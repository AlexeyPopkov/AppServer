import React, { useEffect } from "react";
import { connect } from "react-redux";
import PropTypes from "prop-types";
import { withRouter } from "react-router";
import { isMobile } from "react-device-detect";
import { RequestLoader, toastr } from "asc-web-components";
import { PageLayout, utils } from "asc-web-common";
import { withTranslation, I18nextProvider } from "react-i18next";
import {
  ArticleBodyContent,
  ArticleHeaderContent,
  ArticleMainButtonContent
} from "../../Article";
import {
  SectionBodyContent,
  SectionFilterContent,
  SectionHeaderContent,
  SectionPagingContent
} from "./Section";
import {
  clearProgressData,
  deselectFile,
  fetchFiles,
  getFolder,
  getProgress,
  selectFile,
  setDragging,
  setFilter,
  setNewTreeFilesBadge,
  setProgressBarData,
  setSelected,
  setTreeFolders
} from "../../../store/files/actions";
import {
  loopTreeFolders,
  checkFolderType
} from "../../../store/files/selectors";
import store from "../../../store/store";
import { ConvertDialog } from "../../dialogs";
import { startUpload, onConvert, setDialogVisible } from "./FilesUploader";
import { createI18N } from "../../../helpers/i18n";
const i18n = createI18N({
  page: "Home",
  localesPath: "pages/Home"
});
const { changeLanguage } = utils;

class PureHome extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      isHeaderVisible: false,
      isHeaderIndeterminate: false,
      isHeaderChecked: false,

      overwriteSetting: false,
      uploadOriginalFormatSetting: false,
      hideWindowSetting: false,

      files: [],
      uploadedFiles: 0,
      percent: 0,

      uploadStatus: null,
      uploaded: true,
      uploadToFolder: null
    };
  }

  renderGroupButtonMenu = () => {
    const { files, selection, selected, setSelected, folders } = this.props;

    const headerVisible = selection.length > 0;
    const headerIndeterminate =
      headerVisible &&
      selection.length > 0 &&
      selection.length < files.length + folders.length;
    const headerChecked =
      headerVisible && selection.length === files.length + folders.length;

    let newState = {};

    if (headerVisible || selected === "close") {
      newState.isHeaderVisible = headerVisible;
      if (selected === "close") {
        setSelected("none");
      }
    }

    newState.isHeaderIndeterminate = headerIndeterminate;
    newState.isHeaderChecked = headerChecked;

    this.setState(newState);
  };

  onDrop = (files, e, uploadToFolder) => {
    const { t, currentFolderId, startUpload } = this.props;
    const folderId = uploadToFolder ? uploadToFolder : currentFolderId;

    this.props.setDragging(false);
    startUpload(files, folderId, t);
  };

  onSectionHeaderContentCheck = checked => {
    this.props.setSelected(checked ? "all" : "none");
  };

  onSectionHeaderContentSelect = selected => {
    this.props.setSelected(selected);
  };

  onClose = () => {
    const { selection, setSelected } = this.props;

    if (!selection.length) {
      setSelected("none");
      this.setState({ isHeaderVisible: false });
    } else {
      setSelected("close");
    }
  };

  onChangeOverwrite = () =>
    this.setState({ overwriteSetting: !this.state.overwriteSetting });

  onChangeOriginalFormat = () =>
    this.setState({
      uploadOriginalFormatSetting: !this.state.uploadOriginalFormatSetting
    });

  onChangeWindowVisible = () =>
    this.setState({ hideWindowSetting: !this.state.hideWindowSetting });

  setNewFilter = () => {
    const { filter, selection, setFilter } = this.props;
    const newFilter = filter.clone();
    for (let item of selection) {
      const expandedIndex = newFilter.treeFolders.findIndex(x => x == item.id);
      if (expandedIndex !== -1) {
        newFilter.treeFolders.splice(expandedIndex, 1);
      }
    }

    setFilter(newFilter);
  };

  loopFilesOperations = (id, destFolderId, isCopy) => {
    const {
      currentFolderId,
      filter,
      getFolder,
      getProgress,
      isRecycleBinFolder,
      progressData,
      setNewTreeFilesBadge,
      setProgressBarData,
      treeFolders
    } = this.props;

    getProgress()
      .then(res => {
        const currentItem = res.find(x => x.id === id);
        if (currentItem && currentItem.progress !== 100) {
          setProgressBarData({
            label: progressData.label,
            percent: currentItem.progress,
            visible: true
          });
          setTimeout(
            () => this.loopFilesOperations(id, destFolderId, isCopy),
            1000
          );
        } else {
          setProgressBarData({
            label: progressData.label,
            percent: 100,
            visible: true
          });
          getFolder(destFolderId)
            .then(data => {
              let newTreeFolders = treeFolders;
              let path = data.pathParts.slice(0);
              let folders = data.folders;
              let foldersCount = data.current.foldersCount;
              loopTreeFolders(path, newTreeFolders, folders, foldersCount);

              if (!isCopy || destFolderId === currentFolderId) {
                fetchFiles(currentFolderId, filter, store.dispatch)
                  .then(data => {
                    if (!isRecycleBinFolder) {
                      newTreeFolders = treeFolders;
                      path = data.selectedFolder.pathParts.slice(0);
                      folders = data.selectedFolder.folders;
                      foldersCount = data.selectedFolder.foldersCount;
                      loopTreeFolders(
                        path,
                        newTreeFolders,
                        folders,
                        foldersCount
                      );
                      setNewTreeFilesBadge(true);
                      setTreeFolders(newTreeFolders);
                    }
                    this.setNewFilter();
                  })
                  .catch(err => {
                    toastr.error(err);
                    clearProgressData(store.dispatch);
                  })
                  .finally(() =>
                    setTimeout(() => clearProgressData(store.dispatch), 5000)
                  );
              } else {
                setProgressBarData({
                  label: progressData.label,
                  percent: 100,
                  visible: true
                });
                setTimeout(() => clearProgressData(store.dispatch), 5000);
                setNewTreeFilesBadge(true);
                setTreeFolders(newTreeFolders);
              }
            })
            .catch(err => {
              toastr.error(err);
              clearProgressData(store.dispatch);
            });
        }
      })
      .catch(err => {
        toastr.error(err);
        clearProgressData(store.dispatch);
      });
  };

  setSelections = items => {
    const {
      selection,
      folders,
      files,
      selectFile,
      deselectFile,
      fileActionId
    } = this.props;

    if (selection.length > items.length) {
      //Delete selection
      const newSelection = [];
      let newFile = null;
      for (let item of items) {
        if (!item) break; // temporary fall protection selection tile

        item = item.split("_");
        if (item[0] === "folder") {
          newFile = selection.find(
            x => x.id === Number(item[1]) && !x.fileExst
          );
        } else if (item[0] === "file") {
          newFile = selection.find(x => x.id === Number(item[1]) && x.fileExst);
        }
        if (newFile) {
          newSelection.push(newFile);
        }
      }

      for (let item of selection) {
        const element = newSelection.find(
          x => x.id === item.id && x.fileExst === item.fileExst
        );
        if (!element) {
          deselectFile(item);
        }
      }
    } else if (selection.length < items.length) {
      //Add selection
      for (let item of items) {
        if (!item) break; // temporary fall protection selection tile

        let newFile = null;
        item = item.split("_");
        if (item[0] === "folder") {
          newFile = folders.find(x => x.id === Number(item[1]) && !x.fileExst);
        } else if (item[0] === "file") {
          newFile = files.find(x => x.id === Number(item[1]) && x.fileExst);
        }
        if (newFile && fileActionId !== newFile.id) {
          const existItem = selection.find(
            x => x.id === newFile.id && x.fileExst === newFile.fileExst
          );
          !existItem && selectFile(newFile);
        }
      }
    } else {
      return;
    }
  };

  componentDidUpdate(prevProps) {
    if (this.props.selection !== prevProps.selection) {
      this.renderGroupButtonMenu();
    }
  }

  render() {
    const {
      isHeaderVisible,
      isHeaderIndeterminate,
      isHeaderChecked,
      selected
      // overwriteSetting,
      // uploadOriginalFormatSetting,
      // hideWindowSetting
    } = this.state;
    const {
      t,
      progressData,
      viewAs,
      isLoading,
      convertDialogVisible
    } = this.props;

    // const progressBarContent = (
    //   <div>
    //     <Checkbox
    //       onChange={this.onChangeOverwrite}
    //       isChecked={overwriteSetting}
    //       label={t("OverwriteSetting")}
    //     />
    //     <Checkbox
    //       onChange={this.onChangeOriginalFormat}
    //       isChecked={uploadOriginalFormatSetting}
    //       label={t("UploadOriginalFormatSetting")}
    //     />
    //     <Checkbox
    //       onChange={this.onChangeWindowVisible}
    //       isChecked={hideWindowSetting}
    //       label={t("HideWindowSetting")}
    //     />
    //   </div>
    // );

    return (
      <>
        {convertDialogVisible && (
          <ConvertDialog
            visible={convertDialogVisible}
            onClose={setDialogVisible}
            onConvert={onConvert}
          />
        )}
        <RequestLoader
          visible={isLoading}
          zIndex={256}
          loaderSize="16px"
          loaderColor={"#999"}
          label={`${t("LoadingProcessing")} ${t("LoadingDescription")}`}
          fontSize="12px"
          fontColor={"#999"}
        />
        <PageLayout
          withBodyScroll
          withBodyAutoFocus={!isMobile}
          uploadFiles
          onDrop={this.onDrop}
          setSelections={this.setSelections}
          onMouseMove={this.onMouseMove}
          showProgressBar={progressData.visible}
          progressBarValue={progressData.percent}
          //progressBarDropDownContent={progressBarContent}
          progressBarLabel={progressData.label}
          viewAs={viewAs}
        >
          <PageLayout.ArticleHeader>
            <ArticleHeaderContent />
          </PageLayout.ArticleHeader>

          <PageLayout.ArticleMainButton>
            <ArticleMainButtonContent />
          </PageLayout.ArticleMainButton>

          <PageLayout.ArticleBody>
            <ArticleBodyContent
              onTreeDrop={this.onDrop}
            />
          </PageLayout.ArticleBody>
          <PageLayout.SectionHeader>
            <SectionHeaderContent
              isHeaderVisible={isHeaderVisible}
              isHeaderIndeterminate={isHeaderIndeterminate}
              isHeaderChecked={isHeaderChecked}
              onCheck={this.onSectionHeaderContentCheck}
              onSelect={this.onSectionHeaderContentSelect}
              onClose={this.onClose}
              loopFilesOperations={this.loopFilesOperations}
            />
          </PageLayout.SectionHeader>

          <PageLayout.SectionFilter>
            <SectionFilterContent />
          </PageLayout.SectionFilter>

          <PageLayout.SectionBody>
            <SectionBodyContent
              isMobile={isMobile}
              selected={selected}
              onChange={this.onRowChange}
              loopFilesOperations={this.loopFilesOperations}
              onDropZoneUpload={this.onDrop}
            />
          </PageLayout.SectionBody>

          <PageLayout.SectionPaging>
            <SectionPagingContent />
          </PageLayout.SectionPaging>
        </PageLayout>
      </>
    );
  }
}

const HomeContainer = withTranslation()(PureHome);

const Home = props => {
  useEffect(() => {
    changeLanguage(i18n);
  }, []);
  return (
    <I18nextProvider i18n={i18n}>
      <HomeContainer {...props} />
    </I18nextProvider>
  );
};

Home.propTypes = {
  files: PropTypes.array,
  history: PropTypes.object.isRequired,
  isLoaded: PropTypes.bool
};

function mapStateToProps(state) {
  const {
    convertDialogVisible,
    fileAction,
    files,
    filter,
    folders,
    progressData,
    selected,
    selectedFolder,
    selection,
    treeFolders,
    viewAs,
    isLoading
  } = state.files;
  const { id } = selectedFolder;
  const indexOfTrash = 3;

  return {
    convertDialogVisible,
    currentFolderId: id,
    fileActionId: fileAction.id,
    files,
    filter,
    folders,
    isLoaded: state.auth.isLoaded,
    isRecycleBinFolder: checkFolderType(id, indexOfTrash, treeFolders),
    progressData,
    selected,
    selection,
    treeFolders,
    viewAs,
    isLoading
  };
}

export default connect(
  mapStateToProps,
  {
    deselectFile,
    getFolder,
    getProgress,
    selectFile,
    setDragging,
    setFilter,
    setNewTreeFilesBadge,
    setProgressBarData,
    setSelected,
    setTreeFolders,
    startUpload
  }
)(withRouter(Home));
