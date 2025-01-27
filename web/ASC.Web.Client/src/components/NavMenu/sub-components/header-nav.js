import React, { useCallback, useState } from "react";
import PropTypes from "prop-types";
import styled from "styled-components";
import NavItem from "./nav-item";
import ProfileActions from "./profile-actions";
import { useTranslation } from "react-i18next";
import { tablet } from "@appserver/components/utils/device";
import { combineUrl } from "@appserver/common/utils";
import { inject, observer } from "mobx-react";
import { withRouter } from "react-router";
import { AppServerConfig } from "@appserver/common/constants";
import config from "../../../../package.json";

const { proxyURL } = AppServerConfig;
const homepage = config.homepage;

const PROXY_HOMEPAGE_URL = combineUrl(proxyURL, homepage);
const ABOUT_URL = combineUrl(PROXY_HOMEPAGE_URL, "/about");
const PROFILE_URL = combineUrl(
  PROXY_HOMEPAGE_URL,
  "/products/people/view/@self"
);

const StyledNav = styled.nav`
  display: flex;
  padding: 0 24px 0 16px;
  align-items: center;
  position: absolute;
  right: 0;
  height: 56px;
  z-index: 190 !important;

  .profile-menu {
    right: 12px;
    top: 66px;

    @media ${tablet} {
      right: 6px;
    }
  }

  & > div {
    margin: 0 0 0 16px;
    padding: 0;
    min-width: 24px;
  }

  @media ${tablet} {
    padding: 0 16px;
  }
`;
const HeaderNav = ({ history, modules, user, logout, isAuthenticated }) => {
  const { t } = useTranslation("NavMenu");
  const onProfileClick = useCallback(() => {
    history.push(PROFILE_URL);
  }, []);

  const onAboutClick = useCallback(() => history.push(ABOUT_URL), []);

  const onLogoutClick = useCallback(() => logout && logout(), [logout]);

  const getCurrentUserActions = useCallback(() => {
    const currentUserActions = [
      {
        key: "ProfileBtn",
        label: t("Profile"),
        onClick: onProfileClick,
        url: PROFILE_URL,
      },
      {
        key: "AboutBtn",
        label: t("AboutCompanyTitle"),
        onClick: onAboutClick,
        url: ABOUT_URL,
      },
      {
        key: "LogoutBtn",
        label: t("LogoutButton"),
        onClick: onLogoutClick,
      },
    ];

    return currentUserActions;
  }, [onProfileClick, onAboutClick, onLogoutClick]);

  //console.log("HeaderNav render");
  return (
    <StyledNav className="profileMenuIcon hidingHeader">
      {modules
        .filter((m) => m.isolateMode)
        .map((m) => (
          <NavItem
            key={m.id}
            iconName={m.iconName}
            iconUrl={m.iconUrl}
            badgeNumber={m.notifications}
            url={m.link}
            onClick={(e) => {
              history.push(m.link);
              e.preventDefault();
            }}
            onBadgeClick={(e) => console.log(m.iconName + "Badge Clicked", e)}
            noHover={true}
          />
        ))}

      {isAuthenticated && user ? (
        <ProfileActions userActions={getCurrentUserActions()} user={user} />
      ) : (
        <></>
      )}
    </StyledNav>
  );
};

HeaderNav.displayName = "HeaderNav";

HeaderNav.propTypes = {
  history: PropTypes.object,
  modules: PropTypes.array,
  user: PropTypes.object,
  logout: PropTypes.func,
  isAuthenticated: PropTypes.bool,
  isLoaded: PropTypes.bool,
};

export default withRouter(
  inject(({ auth }) => {
    const {
      settingsStore,
      userStore,
      isAuthenticated,
      isLoaded,
      language,
      logout,
    } = auth;
    const { defaultPage } = settingsStore;
    const { user } = userStore;

    return {
      user,
      isAuthenticated,
      isLoaded,
      language,
      defaultPage: defaultPage || "/",
      modules: auth.availableModules,
      logout,
    };
  })(observer(HeaderNav))
);
