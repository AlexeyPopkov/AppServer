import React, { useEffect } from "react";
import { connect } from "react-redux";
import PropTypes from "prop-types";
import { withRouter } from "react-router";
import { RequestLoader, Loader } from "asc-web-components";
import { PageLayout, utils, api } from "asc-web-common";
import { withTranslation, I18nextProvider } from "react-i18next";
import {
  ArticleHeaderContent,
  ArticleBodyContent,
  ArticleMainButtonContent
} from "../../Article";
import { SectionHeaderContent, SectionBodyContent } from "./Section";
import { createI18N } from "../../../helpers/i18n";
const i18n = createI18N({
  page: "VersionHistory",
  localesPath: "pages/VersionHistory"
});

const { changeLanguage } = utils;

class PureVersionHistory extends React.Component {
  constructor(props) {
    super(props);

    const { match } = props;
    const { fileId } = match.params;

    this.state = {
      fileId,
      versions: null
    };
  }

  componentDidMount() {
    const { match, t } = this.props;
    const { fileId } = match.params;

    //document.title = `${t("GroupAction")} – ${t("People")}`;

    if (fileId) {
      this.getFileVersions(fileId);
    }
  }

  getFileVersions = fileId => {
    api.files
      .getFileVersionInfo(fileId)
      .then(versions => this.setState({ versions }));
  };

  render() {
    const { versions } = this.state;
    const { t, settings, isLoading } = this.props;

    return (
      <>
        <RequestLoader
          visible={isLoading}
          zIndex={256}
          loaderSize="16px"
          loaderColor={"#999"}
          label={`${t("LoadingProcessing")} ${t("LoadingDescription")}`}
          fontSize="12px"
          fontColor={"#999"}
        />
        {versions ? (
          <PageLayout withBodyScroll={true} withBodyAutoFocus={true}>
            <PageLayout.ArticleHeader>
              <ArticleHeaderContent />
            </PageLayout.ArticleHeader>

            <PageLayout.ArticleMainButton>
              <ArticleMainButtonContent
                startUpload={this.startUpload}
              />
            </PageLayout.ArticleMainButton>

            <PageLayout.ArticleBody>
              <ArticleBodyContent />
            </PageLayout.ArticleBody>

            <PageLayout.SectionHeader>
              <SectionHeaderContent title={versions && versions[0].title} />
            </PageLayout.SectionHeader>

            <PageLayout.SectionBody>
              <SectionBodyContent
                getFileVersions={this.getFileVersions}
                versions={versions}
                culture={settings.culture}
              />
            </PageLayout.SectionBody>
          </PageLayout>
        ) : (
            <PageLayout>
              <PageLayout.ArticleHeader>
                <ArticleHeaderContent />
              </PageLayout.ArticleHeader>

              <PageLayout.ArticleMainButton>
                <ArticleMainButtonContent />
              </PageLayout.ArticleMainButton>

              <PageLayout.ArticleBody>
                <ArticleBodyContent />
              </PageLayout.ArticleBody>

              <PageLayout.SectionBody>
                <Loader className="pageLoader" type="rombs" size="40px" />
              </PageLayout.SectionBody>
            </PageLayout>
          )}
      </>
    );
  }
}

const VersionHistoryContainer = withTranslation()(PureVersionHistory);

const VersionHistory = props => {
  useEffect(() => {
    changeLanguage(i18n);
  }, []);
  return (
    <I18nextProvider i18n={i18n}>
      <VersionHistoryContainer {...props} />
    </I18nextProvider>
  );
};

VersionHistory.propTypes = {
  history: PropTypes.object.isRequired,
  isLoaded: PropTypes.bool
};

function mapStateToProps(state) {
  return {
    settings: state.auth.settings,
    isLoaded: state.auth.isLoaded,
    isLoading: state.files.isLoading
  };
}

export default connect(
  mapStateToProps,
  {}
)(withRouter(VersionHistory));
