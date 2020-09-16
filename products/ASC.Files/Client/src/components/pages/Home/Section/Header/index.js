import React from "react";
import copy from "copy-to-clipboard";
import styled, { css } from "styled-components";
import { withRouter } from "react-router";
import { constants, Headline, store, api } from "asc-web-common";
import { connect } from "react-redux";
import { withTranslation } from "react-i18next";
import {
  ContextMenuButton,
  DropDownItem,
  GroupButtonsMenu,
  IconButton,
  toastr,
  utils
} from "asc-web-components";
import { fetchFiles, setAction, getProgress, setProgressBarData, clearProgressData, setIsLoading } from "../../../../../store/files/actions";
import { default as filesStore } from "../../../../../store/store";
import { EmptyTrashDialog, DeleteDialog, DownloadDialog } from "../../../../dialogs";
import { SharingPanel, OperationsPanel } from "../../../../panels";
import { isCanBeDeleted, checkFolderType, isCanCreate } from "../../../../../store/files/selectors";

const { isAdmin } = store.auth.selectors;
const { FilterType, FileAction } = constants;
const { tablet, desktop } = utils.device;

const StyledContainer = styled.div`
  @media ${desktop} {
    ${props =>
    props.isHeaderVisible &&
    css`
        width: calc(100% + 76px);
      `}
  }

  .header-container {
    position: relative;
    display: flex;
    align-items: center;
    max-width: calc(100vw - 32px);

    .arrow-button {
      margin-right: 16px;
      min-width: 17px;

      @media ${tablet} {
        padding: 8px 0 8px 8px;
        margin-left: -8px;
      }
    }

    .add-button {
      margin-bottom: -1px;
      margin-left: 16px;

      @media ${tablet} {
        margin-left: auto;

        & > div:first-child {
          padding: 8px 8px 8px 8px;
          margin-right: -8px;
        }
      }
    }

    .option-button {
      margin-bottom: -1px;
      margin-left: 16px;

      @media ${tablet} {
        & > div:first-child {
          padding: 8px 8px 8px 8px;
          margin-right: -8px;
        }
      }
    }
  }

  .group-button-menu-container {
    margin: 0 -16px;
    -webkit-tap-highlight-color: rgba(0, 0, 0, 0);
    padding-bottom: 56px;

    @media ${tablet} {
      & > div:first-child {
        ${props =>
    props.isArticlePinned &&
    css`
            width: calc(100% - 240px);
          `}
        position: absolute;
        top: 56px;
        z-index: 180;
      }
    }

    @media ${desktop} {
      margin: 0 -24px;
    }
  }
`;

class SectionHeaderContent extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      showSharingPanel: false,
      showDeleteDialog: false,
      showDownloadDialog: false,
      showEmptyTrashDialog: false,
      showMoveToPanel: false,
      showCopyPanel: false
    };
  }

  onCreate = format => {
    this.props.setAction({
      type: FileAction.Create,
      extension: format,
      id: -1
    });
  };

  createDocument = () => this.onCreate("docx");

  createSpreadsheet = () => this.onCreate("xlsx");

  createPresentation = () => this.onCreate("pptx");

  createFolder = () => this.onCreate();

  uploadToFolder = () => toastr.info("Upload To Folder click");

  getContextOptionsPlus = () => {
    const { t } = this.props;

    return [
      {
        key: "new-document",
        label: t("NewDocument"),
        onClick: this.createDocument
      },
      {
        key: "new-spreadsheet",
        label: t("NewSpreadsheet"),
        onClick: this.createSpreadsheet
      },
      {
        key: "new-presentation",
        label: t("NewPresentation"),
        onClick: this.createPresentation
      },
      {
        key: "new-folder",
        label: t("NewFolder"),
        onClick: this.createFolder
      },
      { key: "separator", isSeparator: true },
      {
        key: "make-invitation-link",
        label: t("UploadToFolder"),
        onClick: this.uploadToFolder,
        disabled: true
      }
    ];
  };

  createLinkForPortalUsers = () => {
    const { currentFolderId } = this.props;
    const { t } = this.props;

    copy(`${window.location.origin}/products/files/filter?folder=${currentFolderId}`);

    toastr.success(t("LinkCopySuccess"));
  }

  onMoveAction = () => this.setState({ showMoveToPanel: !this.state.showMoveToPanel });

  onCopyAction = () => this.setState({ showCopyPanel: !this.state.showCopyPanel });

  loop = url => {
    this.props.getProgress().then(res => {
      if (!url) {
        this.props.setProgressBarData({ visible: true, percent: res[0].progress, label: this.props.t("ArchivingData") });
        setTimeout(() => this.loop(res[0].url), 1000);
      } else {
        setTimeout(() => clearProgressData(filesStore.dispatch), 5000);
        return window.open(url, "_blank");
      }
    }).catch((err) => {
      toastr.error(err);
      clearProgressData(filesStore.dispatch);
    });
  }

  downloadAction = () => {
    const { t, selection, setProgressBarData } = this.props;
    const fileIds = [];
    const folderIds = [];
    const items = [];

    for (let item of selection) {
      if (item.fileExst) {
        fileIds.push(item.id);
        items.push({ id: item.id, fileExst: item.fileExst });
      } else {
        folderIds.push(item.id);
        items.push({ id: item.id });
      }
    }

    setProgressBarData({ visible: true, percent: 0, label: t("ArchivingData") });

    api.files
      .downloadFiles(fileIds, folderIds)
      .then(res => {
        this.loop(res[0].url);
      })
      .catch((err) => {
        toastr.error(err);
        clearProgressData(filesStore.dispatch);
      });
  }

  downloadAsAction = () => this.setState({ showDownloadDialog: !this.state.showDownloadDialog });

  renameAction = () => toastr.info("renameAction click");

  onOpenSharingPanel = () =>
    this.setState({ showSharingPanel: !this.state.showSharingPanel });

  onDeleteAction = () =>
    this.setState({ showDeleteDialog: !this.state.showDeleteDialog });

  onEmptyTrashAction = () =>
    this.setState({ showEmptyTrashDialog: !this.state.showEmptyTrashDialog });

  getContextOptionsFolder = () => {
    const { t } = this.props;
    return [
      {
        key: "sharing-settings",
        label: t("SharingSettings"),
        onClick: this.onOpenSharingPanel,
        disabled: true
      },
      {
        key: "link-portal-users",
        label: t("LinkForPortalUsers"),
        onClick: this.createLinkForPortalUsers,
        disabled: false
      },
      { key: "separator-2", isSeparator: true },
      {
        key: "move-to",
        label: t("MoveTo"),
        onClick: this.onMoveAction,
        disabled: true
      },
      {
        key: "copy",
        label: t("Copy"),
        onClick: this.onCopyAction,
        disabled: true
      },
      {
        key: "download",
        label: t("Download"),
        onClick: this.downloadAction,
        disabled: true
      },
      {
        key: "rename",
        label: t("Rename"),
        onClick: this.renameAction,
        disabled: true
      },
      {
        key: "delete",
        label: t("Delete"),
        onClick: this.onDeleteAction,
        disabled: true
      }
    ];
  };

  onBackToParentFolder = () => {
    const { setIsLoading, parentId, filter } = this.props;
    setIsLoading(true);
    fetchFiles(parentId, filter, filesStore.dispatch).finally(() =>
      setIsLoading(false)
    );
  };

  onSelectorSelect = (item) => {
    const { onSelect } = this.props;

    onSelect && onSelect(item.key);
  }

  render() {
    //console.log("Body header render");

    const {
      t,
      selection,
      isHeaderVisible,
      onClose,
      isRecycleBinFolder,
      isHeaderChecked,
      isHeaderIndeterminate,
      deleteDialogVisible,
      folder,
      onCheck,
      title,
      currentFolderId,
      setIsLoading,
      isLoading,
      getProgress,
      loopFilesOperations,
      setProgressBarData,
      isCanCreate
    } = this.props;

    const {
      showDeleteDialog,
      showSharingPanel,
      showEmptyTrashDialog,
      showDownloadDialog,
      showMoveToPanel,
      showCopyPanel
    } = this.state;

    const isItemsSelected = selection.length;
    const isOnlyFolderSelected = selection.every(
      selected => !selected.fileType
    );

    const accessItem = selection.find(x => x.access === 1 || x.access === 0);
    const shareDisable = !accessItem;

    const menuItems = [
      {
        label: t("LblSelect"),
        isDropdown: true,
        isSeparator: true,
        isSelect: true,
        fontWeight: "bold",
        children: [
          <DropDownItem
            key="all"
            label={t("All")}
            data-index={0}
          />,
          <DropDownItem
            key={FilterType.FoldersOnly}
            label={t("Folders")}
            data-index={1}
          />,
          <DropDownItem
            key={FilterType.DocumentsOnly}
            label={t("Documents")}
            data-index={2}
          />,
          <DropDownItem
            key={FilterType.PresentationsOnly}
            label={t("Presentations")}
            data-index={3}
          />,
          <DropDownItem
            key={FilterType.SpreadsheetsOnly}
            label={t("Spreadsheets")}
            data-index={4}
          />,
          <DropDownItem
            key={FilterType.ImagesOnly}
            label={t("Images")}
            data-index={5}
          />,
          <DropDownItem
            key={FilterType.MediaOnly}
            label={t("Media")}
            data-index={6}
          />,
          <DropDownItem
            key={FilterType.ArchiveOnly}
            label={t("Archives")}
            data-index={7}
          />,
          <DropDownItem
            key={FilterType.FilesOnly}
            label={t("AllFiles")}
            data-index={8}
          />
        ],
        onSelect: this.onSelectorSelect
      },
      {
        label: t("Share"),
        disabled: shareDisable,
        onClick: this.onOpenSharingPanel
      },
      {
        label: t("Download"),
        disabled: !isItemsSelected,
        onClick: this.downloadAction
      },
      {
        label: t("DownloadAs"),
        disabled: !isItemsSelected || isOnlyFolderSelected,
        onClick: this.downloadAsAction
      },
      {
        label: t("MoveTo"),
        disabled: !isItemsSelected,
        onClick: this.onMoveAction
      },
      {
        label: t("Copy"),
        disabled: !isItemsSelected,
        onClick: this.onCopyAction
      },
      {
        label: t("Delete"),
        disabled: !isItemsSelected || !deleteDialogVisible,
        onClick: this.onDeleteAction
      }
    ];

    if (isRecycleBinFolder) {
      menuItems.push({
        label: t("EmptyRecycleBin"),
        onClick: this.onEmptyTrashAction
      });

      menuItems.splice(4, 2, {
        label: t("Restore"),
        onClick: this.onMoveAction
      });

      menuItems.splice(1, 1);
    }

    const operationsPanelProps = {
      setIsLoading,
      isLoading,
      loopFilesOperations
    };

    return (
      <StyledContainer isHeaderVisible={isHeaderVisible}>
        {isHeaderVisible ? (
          <div className="group-button-menu-container">
            <GroupButtonsMenu
              checked={isHeaderChecked}
              isIndeterminate={isHeaderIndeterminate}
              onChange={onCheck}
              menuItems={menuItems}
              visible={isHeaderVisible}
              moreLabel={t("More")}
              closeTitle={t("CloseButton")}
              onClose={onClose}
              selected={menuItems[0].label}
            />
          </div>
        ) : (
            <div className="header-container">
              {folder && (
                <IconButton
                  iconName="ArrowPathIcon"
                  size="17"
                  color="#A3A9AE"
                  hoverColor="#657077"
                  isFill={true}
                  onClick={this.onBackToParentFolder}
                  className="arrow-button"
                />
              )}
              <Headline
                className="headline-header"
                type="content"
                truncate={true}
              >
                {title}
              </Headline>
              {folder && isCanCreate ? (
                <>
                  <ContextMenuButton
                    className="add-button"
                    directionX="right"
                    iconName="PlusIcon"
                    size={17}
                    color="#A3A9AE"
                    hoverColor="#657077"
                    isFill
                    getData={this.getContextOptionsPlus}
                    isDisabled={false}
                  />
                  <ContextMenuButton
                    className="option-button"
                    directionX="right"
                    iconName="VerticalDotsIcon"
                    size={17}
                    color="#A3A9AE"
                    hoverColor="#657077"
                    isFill
                    getData={this.getContextOptionsFolder}
                    isDisabled={false}
                  />
                </>
              ) : (
                isCanCreate && (
                  <ContextMenuButton
                    className="add-button"
                    directionX="right"
                    iconName="PlusIcon"
                    size={17}
                    color="#A3A9AE"
                    hoverColor="#657077"
                    isFill
                    getData={this.getContextOptionsPlus}
                    isDisabled={false}
                  />
                )
              )}
            </div>
          )}

        {showDeleteDialog && (
          <DeleteDialog
            {...operationsPanelProps}
            isRecycleBinFolder={isRecycleBinFolder}
            visible={showDeleteDialog}
            onClose={this.onDeleteAction}
            selection={selection}
          />
        )}

        {showEmptyTrashDialog && (
          <EmptyTrashDialog
            {...operationsPanelProps}
            setProgressBarData={setProgressBarData}
            getProgress={getProgress}
            currentFolderId={currentFolderId}
            visible={showEmptyTrashDialog}
            onClose={this.onEmptyTrashAction}
          />
        )}

        {showSharingPanel && (
          <SharingPanel
            onClose={this.onOpenSharingPanel}
            visible={showSharingPanel}
          />
        )}

        {showMoveToPanel && (
          <OperationsPanel
            {...operationsPanelProps}
            isCopy={false}
            visible={showMoveToPanel}
            onClose={this.onMoveAction}
          />
        )}

        {showCopyPanel && (
          <OperationsPanel
            {...operationsPanelProps}
            isCopy={true}
            visible={showCopyPanel}
            onClose={this.onCopyAction}
          />
        )}

        {showDownloadDialog && (
          <DownloadDialog
            visible={showDownloadDialog}
            onClose={this.downloadAsAction}
            onDownloadProgress={this.loop}
          />
        )}
      </StyledContainer>
    );
  }
}

const mapStateToProps = state => {
  const {
    selectedFolder,
    selection,
    treeFolders,
    filter,
    isLoading
  } = state.files;
  const { parentId, title, id } = selectedFolder;
  const { user } = state.auth;

  const indexOfTrash = 3;
  user.rights = { icon: "AccessEditIcon", rights: "FullAccess" };

  return {
    folder: parentId !== 0,
    isAdmin: isAdmin(user),
    isRecycleBinFolder: checkFolderType(id, indexOfTrash, treeFolders),
    parentId,
    selection,
    title,
    filter,
    deleteDialogVisible: isCanBeDeleted(selectedFolder, user),
    currentFolderId: id,
    isLoading,
    isCanCreate: isCanCreate(selectedFolder, user),
  };
};

export default connect(mapStateToProps, { setAction, getProgress, setProgressBarData, setIsLoading })(
  withTranslation()(withRouter(SectionHeaderContent))
);
