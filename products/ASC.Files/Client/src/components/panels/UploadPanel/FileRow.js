import React from "react";
import styled from "styled-components";
import Tooltip from "@appserver/components/tooltip";
import Row from "@appserver/components/row";
import Text from "@appserver/components/text";
import Link from "@appserver/components/link";
import LoadingButton from "./LoadingButton";
import ShareButton from "./ShareButton";
import LoadErrorIcon from "../../../../public/images/load.error.react.svg";
import { inject, observer } from "mobx-react";

const StyledFileRow = styled(Row)`
  margin: 0 16px;
  width: calc(100% - 16px);
  box-sizing: border-box;
  font-weight: 600;

  .upload_panel-icon {
    margin-left: auto;
    padding-left: 16px;
    line-height: 24px;
    display: flex;
    align-items: center;
    flex-direction: row-reverse;

    svg {
      width: 16px;
      height: 16px;
    }
  }

  .img_error {
    filter: grayscale(1);
  }

  .__react_component_tooltip.type-light {
    background-color: #f8f7bf !important;
    box-shadow: none;
    -moz-box-shadow: none;
    -webkit-box-shadow: none;
  }
  .__react_component_tooltip.place-left::after {
    border-left: 6px solid #f8f7bf !important;
  }

  .__react_component_tooltip.place-right::after {
    border-right: 6px solid #f8f7bf !important;
  }

  .__react_component_tooltip.place-top::after {
    border-top: 6px solid #f8f7bf !important;
  }

  .__react_component_tooltip.place-bottom::after {
    border-bottom: 6px solid #f8f7bf !important;
  }
`;

const FileRow = (props) => {
  const {
    t,
    item,
    uploaded,
    cancelCurrentUpload,
    //onMediaClick,
    currentFileUploadProgress,
    fileIcon,
    isMedia,
    ext,
    name,
  } = props;

  const onCancelCurrentUpload = (e) => {
    //console.log("cancel upload ", e);
    const id = e.currentTarget.dataset.id;
    return cancelCurrentUpload(id);
  };

  // const onMediaClick = (id) => {
  //   console.log("id", id);
  //   const item = { visible: true, id: id };
  //   this.props.setMediaViewerData(item);
  // };

  return (
    <>
      <StyledFileRow
        className="download-row"
        key={item.uniqueId}
        checkbox={false}
        element={
          <img className={item.error && "img_error"} src={fileIcon} alt="" />
        }
      >
        <>
          {item.fileId ? (
            isMedia ? (
              <Text
                fontWeight="600"
                color={item.error && "#A3A9AE"}
                truncate
                // MediaViewer doesn't work
                /*onClick={() => onMediaClick(item.fileId)}*/
              >
                {name}
              </Text>
            ) : (
              <Link
                fontWeight="600"
                color={item.error && "#A3A9AE"}
                truncate
                href={item.fileInfo ? item.fileInfo.webUrl : ""}
                target="_blank"
              >
                {name}
              </Link>
            )
          ) : (
            <Text fontWeight="600" color={item.error && "#A3A9AE"} truncate>
              {name}
            </Text>
          )}
          {ext ? (
            <Text fontWeight="600" color="#A3A9AE">
              {ext}
            </Text>
          ) : (
            <></>
          )}
          {item.fileId ? (
            <ShareButton uniqueId={item.uniqueId} />
          ) : item.error || (!item.fileId && uploaded) ? (
            <div className="upload_panel-icon">
              {" "}
              <LoadErrorIcon
                size="medium"
                data-for="errorTooltip"
                data-tip={item.error || t("UnknownError")}
              />
              <Tooltip
                id="errorTooltip"
                offsetTop={64}
                getContent={(dataTip) => <Text fontSize="13px">{dataTip}</Text>}
                effect="float"
                place="left"
                maxWidth={320}
                color="#f8f7bf"
              />
            </div>
          ) : (
            <div
              className="upload_panel-icon"
              data-id={item.uniqueId}
              onClick={onCancelCurrentUpload}
            >
              <LoadingButton percent={currentFileUploadProgress} />
            </div>
          )}
        </>
      </StyledFileRow>
    </>
  );
};

export default inject(
  ({ filesStore, formatsStore, uploadDataStore }, { item }) => {
    let ext;
    let name;
    let splitted;
    if (item.file) {
      splitted = item.file.name.split(".");
      ext = splitted.length > 1 ? "." + splitted.pop() : "";
      name = splitted[0];
    } else {
      ext = item.fileInfo.fileExst;
      splitted = item.fileInfo.title.split(".");
      name = splitted[0];
    }

    const { iconFormatsStore, mediaViewersFormatsStore } = formatsStore;
    const { cancelCurrentUpload } = filesStore;
    const { uploaded, primaryProgressDataStore } = uploadDataStore;
    const { loadingFile: file } = primaryProgressDataStore;
    const isMedia = mediaViewersFormatsStore.isMediaOrImage(ext);
    const fileIcon = iconFormatsStore.getIconSrc(ext, 24);

    const loadingFile = !file || !file.uniqueId ? null : file;

    const currentFileUploadProgress =
      file && loadingFile.uniqueId === item.uniqueId
        ? loadingFile.percent
        : null;

    return {
      currentFileUploadProgress,
      uploaded,
      isMedia,
      fileIcon,
      ext,
      name,
      loadingFile,

      cancelCurrentUpload,
    };
  }
)(observer(FileRow));
