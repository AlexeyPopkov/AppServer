import React, { useEffect, useState } from "react";
import { withRouter } from "react-router";
import PageLayout from "@appserver/common/components/PageLayout";
import Loaders from "@appserver/common/components/Loaders";
import { showLoader, hideLoader } from "@appserver/common/utils";
import {
  ArticleHeaderContent,
  ArticleBodyContent,
  ArticleMainButtonContent,
} from "../../Article";
import { SectionHeaderContent, SectionBodyContent } from "./Section";
import { withTranslation } from "react-i18next";
import { setDocumentTitle } from "../../../helpers/utils";
import { inject, observer } from "mobx-react";

const PureSettings = ({
  match,
  t,
  isLoading,
  isLoadedSettingsTree,
  setFirstLoad,
}) => {
  const [title, setTitle] = useState("");
  const { setting } = match.params;

  useEffect(() => {
    setFirstLoad(false);
  }, [setFirstLoad]);

  useEffect(() => {
    switch (setting) {
      case "common":
        setTitle("CommonSettings");
        break;
      case "admin":
        setTitle("AdminSettings");
        break;
      case "thirdParty":
        setTitle("ThirdPartySettings");
        break;
      default:
        setTitle("CommonSettings");
        break;
    }
  }, [setting]);

  useEffect(() => {
    if (isLoading) {
      showLoader();
    } else {
      hideLoader();
    }
  }, [isLoading]);

  //console.log("render settings");

  useEffect(() => {
    setDocumentTitle(t(`${title}`));
  }, [title, t]);

  return (
    <>
      <PageLayout>
        <PageLayout.ArticleHeader>
          <ArticleHeaderContent />
        </PageLayout.ArticleHeader>

        <PageLayout.ArticleMainButton>
          <ArticleMainButtonContent isDisabled={true} />
        </PageLayout.ArticleMainButton>

        <PageLayout.ArticleBody>
          <ArticleBodyContent />
        </PageLayout.ArticleBody>

        <PageLayout.SectionHeader>
          {(!isLoadedSettingsTree && isLoading) || isLoading ? (
            <Loaders.SectionHeader />
          ) : (
            <SectionHeaderContent title={t(`${title}`)} />
          )}
        </PageLayout.SectionHeader>

        <PageLayout.SectionBody>
          {(!isLoadedSettingsTree && isLoading) || isLoading ? (
            setting === "thirdParty" ? (
              <Loaders.Rows />
            ) : (
              <Loaders.SettingsFiles />
            )
          ) : (
            <SectionBodyContent setting={setting} t={t} />
          )}
        </PageLayout.SectionBody>
      </PageLayout>
    </>
  );
};

const Settings = withTranslation("Settings")(PureSettings);

export default inject(
  ({ initFilesStore, filesStore, settingsStore, treeFoldersStore }) => {
    const { isLoading } = initFilesStore;
    const { setFirstLoad } = filesStore;
    const { setSelectedNode } = treeFoldersStore;
    const { getFilesSettings, isLoadedSettingsTree } = settingsStore;

    return {
      isLoading,
      isLoadedSettingsTree,

      setFirstLoad,
      setSelectedNode,
      getFilesSettings,
    };
  }
)(withRouter(observer(Settings)));
