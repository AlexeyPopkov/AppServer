import React from "react";
import styled, { css } from "styled-components";
import Link from "@appserver/components/link";
import { withTranslation } from "react-i18next";
import { isMobile } from "react-device-detect";

import { inject, observer } from "mobx-react";
import { withRouter } from "react-router";
import { combineUrl } from "@appserver/common/utils";
import { AppServerConfig } from "@appserver/common/constants";
import config from "../../../../package.json";

const StyledThirdParty = styled.div`
  margin-top: 42px;
  ${isMobile &&
  css`
    margin-bottom: 64px;
  `}
  .tree-thirdparty-list {
    padding-top: 3px;
    display: flex;
    max-width: 200px;

    div {
      height: 26px;
      width: 100%;
      background: #eceef1;
      text-align: center;
      margin-right: 1px;
      color: #818b91;
      :first-of-type {
        border-radius: 3px 0 0 3px;
      }
      :last-of-type {
        border-radius: 0 3px 3px 0;

        img {
          margin-top: 4px;
        }
      }

      img {
        padding: 4px 6px 0 4px;
      }

      @media (max-width: 1024px) {
        height: 32px;
        margin-right: 0px;
        :first-of-type {
          border-radius: 3px 0 0 3px;
          padding-left: 5px;
        }
        :last-of-type {
          border-radius: 0 3px 3px 0;
          padding-right: 5px;

          img {
            margin-top: 7px;
          }
        }

        img {
          padding: 7px 4px 0 4px;
        }
      }

      &:hover {
        cursor: pointer;
      }
    }
  }
`;

const ServiceItem = (props) => {
  const { capability, src, ...rest } = props;

  const capabilityName = capability[0];
  const capabilityLink = capability[1] ? capability[1] : "";

  const dataProps = {
    "data-link": capabilityLink,
    "data-title": capabilityName,
  };

  return (
    <div {...dataProps} {...rest}>
      <img src={src} alt="" />
    </div>
  );
};

const PureThirdPartyListContainer = ({
  t,
  googleConnectItem,
  boxConnectItem,
  dropboxConnectItem,
  oneDriveConnectItem,
  nextCloudConnectItem,
  webDavConnectItem,
  setConnectItem,
  setThirdPartyDialogVisible,
  setSelectedNode,
  setSelectedFolder,
  getOAuthToken,
  openConnectWindow,
  history,
}) => {
  const redirectAction = () => {
    const thirdPartyUrl = "/settings/thirdParty";
    if (history.location.pathname.indexOf(thirdPartyUrl) === -1) {
      setSelectedNode(["thirdParty"]);
      setSelectedFolder(null);
      return history.push(
        combineUrl(AppServerConfig.proxyURL, config.homepage, thirdPartyUrl)
      );
    }
  };

  const onConnect = (e) => {
    const data = e.currentTarget.dataset;

    if (data.link) {
      let authModal = window.open(
        "",
        "Authorization",
        "height=600, width=1020"
      );
      openConnectWindow(data.title, authModal).then((modal) =>
        getOAuthToken(modal).then((token) => {
          const serviceData = {
            title: data.title,
            provider_key: data.title,
            link: data.link,
            token,
          };
          setConnectItem(serviceData);
        })
      );
    } else {
      setConnectItem(data);
    }

    onShowConnectPanel();
  };

  const onShowConnectPanel = () => {
    //setThirdPartyDialogVisible((prev) => !prev); TODO:
    setThirdPartyDialogVisible(true);
    redirectAction();
  };

  return (
    <StyledThirdParty>
      <Link
        color="#555F65"
        fontSize="14px"
        fontWeight={600}
        onClick={onShowConnectPanel}
      >
        {t("AddAccount")}
      </Link>
      <div className="tree-thirdparty-list">
        {googleConnectItem && (
          <ServiceItem
            capability={googleConnectItem}
            src="images/services/google_drive.svg"
            onClick={onConnect}
          />
        )}
        {boxConnectItem && (
          <ServiceItem
            capability={boxConnectItem}
            src="images/services/box.svg"
            onClick={onConnect}
          />
        )}
        {dropboxConnectItem && (
          <ServiceItem
            capability={dropboxConnectItem}
            src="images/services/dropbox.svg"
            onClick={onConnect}
          />
        )}
        {oneDriveConnectItem && (
          <ServiceItem
            capability={oneDriveConnectItem}
            src="images/services/onedrive.svg"
            onClick={onConnect}
          />
        )}
        {nextCloudConnectItem && (
          <ServiceItem
            capability={nextCloudConnectItem}
            src="images/services/nextcloud.svg"
            onClick={onConnect}
          />
        )}
        {webDavConnectItem && (
          <ServiceItem
            capability={webDavConnectItem}
            src="images/services/more.svg"
            onClick={onConnect}
          />
        )}
      </div>
    </StyledThirdParty>
  );
};

const ThirdPartyList = withTranslation("Article")(
  withRouter(PureThirdPartyListContainer)
);

export default inject(
  ({
    initFilesStore,
    settingsStore,
    treeFoldersStore,
    selectedFolderStore,
    dialogsStore,
  }) => {
    const { setIsLoading } = initFilesStore;
    const { setSelectedFolder } = selectedFolderStore;
    const { setSelectedNode } = treeFoldersStore;
    const {
      googleConnectItem,
      boxConnectItem,
      dropboxConnectItem,
      oneDriveConnectItem,
      nextCloudConnectItem,
      webDavConnectItem,
      getOAuthToken,
      openConnectWindow,
    } = settingsStore.thirdPartyStore;

    const { setConnectItem, setThirdPartyDialogVisible } = dialogsStore;

    return {
      googleConnectItem,
      boxConnectItem,
      dropboxConnectItem,
      oneDriveConnectItem,
      nextCloudConnectItem,
      webDavConnectItem,

      setIsLoading,
      setSelectedFolder,
      setSelectedNode,
      setConnectItem,
      setThirdPartyDialogVisible,
      getOAuthToken,
      openConnectWindow,
    };
  }
)(observer(ThirdPartyList));
