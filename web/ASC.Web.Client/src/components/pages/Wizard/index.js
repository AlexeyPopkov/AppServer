import React, { Component } from "react";
import { withRouter } from "react-router";
import styled from "styled-components";
import { withTranslation } from "react-i18next";
import PropTypes from "prop-types";
import axios from "axios";

import PageLayout from "@appserver/common/components/PageLayout";
import ErrorContainer from "@appserver/common/components/ErrorContainer";
import history from "@appserver/common/history";
import { combineUrl, createPasswordHash } from "@appserver/common/utils";
import Loader from "@appserver/components/loader";
import { tablet } from "@appserver/components/utils/device";
import { EmailSettings } from "@appserver/components/utils/email";

import HeaderContainer from "./sub-components/header-container";
import ButtonContainer from "./sub-components/button-container";
import SettingsContainer from "./sub-components/settings-container";
import InputContainer from "./sub-components/input-container";
import ModalContainer from "./sub-components/modal-dialog-container";

import { setDocumentTitle } from "../../../helpers/utils";
import { inject, observer } from "mobx-react";
import { AppServerConfig } from "@appserver/common/constants";

const emailSettings = new EmailSettings();
emailSettings.allowDomainPunycode = true;

const WizardContainer = styled.div`
  width: 960px;
  margin: 0 auto;
  margin-top: 63px;

  .wizard-form {
    margin-top: 33px;
    display: grid;
    grid-template-columns: 1fr;
    grid-row-gap: 32px;
  }

  @media ${tablet} {
    width: 100%;
    max-width: 480px;
  }

  @media (max-width: 520px) {
    width: calc(100% - 32px);
  }
`;

class Body extends Component {
  constructor(props) {
    super(props);

    this.state = {
      password: "",
      isValidPass: false,
      errorLoading: false,
      errorMessage: null,
      errorInitWizard: null,
      sending: false,
      visibleModal: false,
      emailValid: false,
      email: "",
      changeEmail: "",
      license: false,
      languages: null,
      timezones: null,
      selectLanguage: null,
      selectTimezone: null,

      emailNeeded: true,
      emailOwner: "fake@mail.com",

      hasErrorEmail: false,
      hasErrorPass: false,
      hasErrorLicense: false,

      checkingMessages: [],
    };
  }

  async componentDidMount() {
    const {
      t,
      wizardToken,
      getPortalPasswordSettings,
      getPortalCultures,
      getPortalTimezones,
      setIsWizardLoaded,
      getMachineName,
      getIsRequiredLicense,
      history,
    } = this.props;

    window.addEventListener("keyup", this.onKeyPressHandler);

    if (!wizardToken) {
      history.push(combineUrl(AppServerConfig.proxyURL, "/"));
    } else {
      await axios
        .all([
          getPortalPasswordSettings(wizardToken),
          getMachineName(wizardToken),
          getIsRequiredLicense(),
          getPortalTimezones(wizardToken).then(() => {
            const { timezones, timezone } = this.props;
            const zones = this.mapTimezonesToArray(timezones);
            const select = zones.filter((zone) => zone.key === timezone);
            this.setState({
              timezones: zones,
              selectTimezone: {
                key: select[0].key,
                label: select[0].label,
              },
            });
          }),
          getPortalCultures().then(() => {
            const { cultures, culture } = this.props;
            const languages = this.mapCulturesToArray(cultures, t);
            let select = languages.filter((lang) => lang.key === culture);
            if (!select.length)
              select = languages.filter((lang) => lang.key === "en-US");
            this.setState({
              languages: languages,
              selectLanguage: {
                key: select[0].key,
                label: select[0].label,
              },
            });
          }),
        ])
        .then(() => {
          setIsWizardLoaded(true);
          setDocumentTitle(t("WizardTitle"));
        })
        .catch((e) => {
          this.setState({
            errorInitWizard: e,
          });
        });
    }
  }

  shouldComponentUpdate(nextProps, nextState) {
    if (
      nextProps.isWizardLoaded === true ||
      nextState.errorInitWizard !== null
    ) {
      return true;
    } else {
      return false;
    }
  }

  componentWillUnmount() {
    window.removeEventListener("keyup", this.onKeyPressHandler);
  }

  mapTimezonesToArray = (timezones) => {
    return timezones.map((timezone) => {
      return { key: timezone.id, label: timezone.displayName };
    });
  };

  mapCulturesToArray = (cultures, t) => {
    return cultures.map((culture) => {
      return { key: culture, label: t(`Culture_${culture}`) };
    });
  };

  onKeyPressHandler = (e) => {
    if (e.key === "Enter") this.onContinueHandler();
  };

  isValidPassHandler = (val) => this.setState({ isValidPass: val });

  onChangePassword = (e) =>
    this.setState({ password: e.target.value, hasErrorPass: false });

  onClickChangeEmail = () => this.setState({ visibleModal: true });

  onEmailChangeHandler = (result) => {
    const { emailNeeded } = this.state;

    emailNeeded
      ? this.setState({
          emailValid: result.isValid,
          email: result.value,
          hasErrorEmail: false,
        })
      : this.setState({
          emailValid: result.isValid,
          changeEmail: result.value,
        });
  };

  onChangeLicense = () => this.setState({ license: !this.state.license });

  onContinueHandler = () => {
    const valid = this.checkingValid();

    if (valid) {
      const {
        setPortalOwner,
        wizardToken,
        hashSettings,
        getPortalSettings,
        setWizardComplete,
      } = this.props;

      const {
        password,
        email,
        selectLanguage,
        selectTimezone,
        emailOwner,
      } = this.state;

      this.setState({ sending: true });

      const emailTrim = email ? email.trim() : emailOwner.trim();
      const analytics = true;

      // console.log(emailTrim, password, selectLanguage.key, selectTimezone.key, analytics, wizardToken);
      const hash = createPasswordHash(password, hashSettings);

      setPortalOwner(
        emailTrim,
        hash,
        selectLanguage.key,
        selectTimezone.key,
        wizardToken,
        analytics
      )
        .then(() => {
          setWizardComplete();
          getPortalSettings();
        })
        .then(() =>
          history.push(combineUrl(AppServerConfig.proxyURL, "/login"))
        )
        .catch((e) =>
          this.setState({
            errorLoading: true,
            sending: false,
            errorMessage: e,
          })
        );
    } else {
      this.setState({ visibleModal: true });
    }
  };

  checkingValid = () => {
    const { t, isLicenseRequired, licenseUpload } = this.props;
    const { isValidPass, emailValid, license, emailNeeded } = this.state;

    let checkingMessages = [];
    if (!isValidPass) {
      checkingMessages.push(t("ErrorPassword"));
      this.setState({ hasErrorPass: true, checkingMessages: checkingMessages });
    }
    if (!license) {
      checkingMessages.push(t("ErrorLicenseRead"));
      this.setState({ checkingMessages: checkingMessages });
    }

    if (emailNeeded && !isLicenseRequired) {
      if (!emailValid) {
        checkingMessages.push(t("ErrorEmail"));
        this.setState({
          hasErrorEmail: true,
          checkingMessages: checkingMessages,
        });
      }

      if (isValidPass && emailValid && license) {
        return true;
      }
    }

    if (emailNeeded && isLicenseRequired) {
      if (!emailValid) {
        checkingMessages.push(t("ErrorEmail"));
        this.setState({
          hasErrorEmail: true,
          checkingMessages: checkingMessages,
        });
      }

      if (!licenseUpload) {
        checkingMessages.push(t("ErrorUploadLicenseFile"));
        this.setState({
          hasErrorLicense: true,
          checkingMessages: checkingMessages,
        });
      }

      if (isValidPass && emailValid && license && licenseUpload) {
        return true;
      }
    }

    if (!emailNeeded && isLicenseRequired) {
      if (!licenseUpload) {
        checkingMessages.push(t("ErrorUploadLicenseFile"));
        this.setState({
          hasErrorLicense: true,
          checkingMessages: checkingMessages,
        });
      }

      if (isValidPass && license && licenseUpload) {
        return true;
      }
    }

    return false;
  };

  onSaveEmailHandler = () => {
    const { changeEmail, emailValid } = this.state;
    if (emailValid && changeEmail) {
      this.setState({ email: changeEmail });
    }
    this.setState({ visibleModal: false });
  };

  onCloseModal = () => {
    this.setState({
      visibleModal: false,
      errorLoading: false,
      errorMessage: null,
    });
  };

  onSelectTimezoneHandler = (el) => this.setState({ selectTimezone: el });

  onSelectLanguageHandler = (lang) =>
    this.setState({
      selectLanguage: {
        key: lang.key,
        label: lang.label,
      },
    });

  onInputFileHandler = (file) => {
    const {
      setLicense,
      wizardToken,
      licenseUpload,
      resetLicenseUploaded,
    } = this.props;

    if (licenseUpload) resetLicenseUploaded();

    this.setState({ hasErrorLicense: false });

    let fd = new FormData();
    fd.append("files", file);

    setLicense(wizardToken, fd).catch((e) =>
      this.setState({
        errorLoading: true,
        errorMessage: e,
        hasErrorLicense: true,
      })
    );
  };

  render() {
    const {
      t,
      isWizardLoaded,
      machineName,
      passwordSettings,
      culture,
      isLicenseRequired,
      urlLicense,
    } = this.props;

    const {
      sending,
      selectLanguage,
      license,
      selectTimezone,
      languages,
      timezones,
      emailNeeded,
      email,
      emailOwner,
      password,
      errorLoading,
      visibleModal,
      errorMessage,
      errorInitWizard,
      changeEmail,
      hasErrorEmail,
      hasErrorPass,
      hasErrorLicense,
      checkingMessages,
    } = this.state;

    console.log("wizard render");

    if (errorInitWizard) {
      return (
        <ErrorContainer
          headerText={t("ErrorInitWizardHeader")}
          bodyText={t("ErrorInitWizard")}
          buttonText={t("ErrorInitWizardButton")}
          buttonUrl="/"
        />
      );
    } else if (isWizardLoaded) {
      return (
        <WizardContainer>
          <ModalContainer
            t={t}
            errorLoading={errorLoading}
            visibleModal={visibleModal}
            errorMessage={errorMessage}
            emailOwner={changeEmail ? changeEmail : emailOwner}
            settings={emailSettings}
            checkingMessages={checkingMessages}
            onEmailChangeHandler={this.onEmailChangeHandler}
            onSaveEmailHandler={this.onSaveEmailHandler}
            onCloseModal={this.onCloseModal}
          />

          <HeaderContainer t={t} />

          <form className="wizard-form">
            <InputContainer
              t={t}
              settingsPassword={passwordSettings}
              emailNeeded={emailNeeded}
              password={password}
              license={license}
              settings={emailSettings}
              isLicenseRequired={isLicenseRequired}
              hasErrorEmail={hasErrorEmail}
              hasErrorPass={hasErrorPass}
              hasErrorLicense={hasErrorLicense}
              urlLicense={urlLicense}
              onChangeLicense={this.onChangeLicense}
              isValidPassHandler={this.isValidPassHandler}
              onChangePassword={this.onChangePassword}
              onInputFileHandler={this.onInputFileHandler}
              onEmailChangeHandler={this.onEmailChangeHandler}
            />

            <SettingsContainer
              t={t}
              selectLanguage={selectLanguage}
              selectTimezone={selectTimezone}
              languages={languages}
              timezones={timezones}
              emailNeeded={emailNeeded}
              emailOwner={emailOwner}
              email={email}
              machineName={machineName}
              portalCulture={culture}
              onClickChangeEmail={this.onClickChangeEmail}
              onSelectLanguageHandler={this.onSelectLanguageHandler}
              onSelectTimezoneHandler={this.onSelectTimezoneHandler}
            />

            <ButtonContainer
              t={t}
              sending={sending}
              onContinueHandler={this.onContinueHandler}
            />
          </form>
        </WizardContainer>
      );
    }
    return <Loader className="pageLoader" type="rombs" size="40px" />;
  }
}

Body.propTypes = {
  culture: PropTypes.string,
  i18n: PropTypes.object,
  isWizardLoaded: PropTypes.bool.isRequired,
  machineName: PropTypes.string.isRequired,
  wizardToken: PropTypes.string,
  passwordSettings: PropTypes.object,
  cultures: PropTypes.array.isRequired,
  timezones: PropTypes.array.isRequired,
  timezone: PropTypes.string.isRequired,
  licenseUpload: PropTypes.string,
};

const WizardWrapper = withTranslation("Wizard")(Body);

const WizardPage = (props) => {
  const { isLoaded } = props;

  return (
    isLoaded && (
      <PageLayout>
        <PageLayout.SectionBody>
          <WizardWrapper {...props} />
        </PageLayout.SectionBody>
      </PageLayout>
    )
  );
};

WizardPage.propTypes = {
  culture: PropTypes.string.isRequired,
  isLoaded: PropTypes.bool,
};

export default inject(({ auth, wizard }) => {
  const {
    passwordSettings,
    culture,
    wizardToken,
    cultures,
    timezones,
    timezone,
    urlLicense,
    hashSettings,
    getPortalSettings,
    setWizardComplete,
    getPortalTimezones,
    getPortalCultures,
    getPortalPasswordSettings,
  } = auth.settingsStore;

  const {
    isWizardLoaded,
    machineName,
    isLicenseRequired,
    licenseUpload,
    setIsWizardLoaded,
    getMachineName,
    getIsRequiredLicense,
    setPortalOwner,
    setLicense,
    resetLicenseUploaded,
  } = wizard;

  return {
    isLoaded: auth.isLoaded,
    culture,
    wizardToken,
    passwordSettings,
    cultures,
    timezones,
    timezone,
    urlLicense,
    hashSettings,
    isWizardLoaded,
    machineName,
    isLicenseRequired,
    licenseUpload,
    getPortalSettings,
    setWizardComplete,
    getPortalPasswordSettings,
    getPortalCultures,
    getPortalTimezones,
    setIsWizardLoaded,
    getMachineName,
    getIsRequiredLicense,
    setPortalOwner,
    setLicense,
    resetLicenseUploaded,
  };
})(withRouter(observer(WizardPage)));
