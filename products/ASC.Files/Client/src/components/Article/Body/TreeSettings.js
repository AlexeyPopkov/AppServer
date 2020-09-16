import React, { useEffect } from "react";
import { connect } from "react-redux";
import { withRouter } from "react-router";
import { TreeMenu, TreeNode, Icons } from "asc-web-components";
import styled from "styled-components";
import { history, utils } from "asc-web-common";
import { withTranslation, I18nextProvider } from "react-i18next";
import { createI18N } from "../../../helpers/i18n";

import {
  setSelectedNode,
  setExpandSettingsTree
} from "../../../store/files/actions";

const i18n = createI18N({
  page: "Settings",
  localesPath: "pages/Settings"
});

const { changeLanguage } = utils;

const StyledTreeMenu = styled(TreeMenu)`
  margin-top: 20px !important;

  .rc-tree-node-selected {
    background: #dfe2e3 !important;
  }

  .settings-node > .rc-tree-node-content-wrapper > .rc-tree-title {
    padding-left: 4px !important;
    width: 100%;
  }
  .rc-tree-treenode-disabled > span:not(.rc-tree-switcher),
  .rc-tree-treenode-disabled > a,
  .rc-tree-treenode-disabled > a span {
    cursor: wait;
  }

  .rc-tree-child-tree {
    margin-left: 31px;
  }
`;

const PureTreeSettings = ({
  match,
  enableThirdParty, 
  isAdmin,
  selectedTreeNode, 
  expandedSetting,
  isLoading,
  setSelectedNode,
  setExpandSettingsTree,
  t
}) => {
  useEffect(() => {
    const { setting } = match.params;
    setSelectedNode([setting]);
    if (setting) setExpandSettingsTree(["settings"]);
  }, [match]);

  const switcherIcon = obj => {
    if (obj.isLeaf) {
      return null;
    }
    if (obj.expanded) {
      return <Icons.ExpanderDownIcon size="scale" isfill color="dimgray" />;
    } else {
      return <Icons.ExpanderRightIcon size="scale" isfill color="dimgray" />;
    }
  };

  const onSelect = section => {
    const path = section[0];

    if (path === "settings") {
      setSelectedNode(["common"]);
      setExpandSettingsTree(section);
      return history.push("/products/files/settings/common");
    }

    setSelectedNode(section);
    return history.push(`/products/files/settings/${path}`);
  };

  const onExpand = data => {
    setExpandSettingsTree(data);
  };

  const renderTreeNode = () => {
    return (
      <TreeNode
        id="settings"
        key="settings"
        title={t("treeSettingsMenuTitle")}
        isLeaf={false}
        icon={<Icons.SettingsIcon size="scale" isfill color="dimgray" />}
      >
        <TreeNode
          className="settings-node"
          id="common-settings"
          key="common"
          isLeaf={true}
          title={t("treeSettingsCommonSettings")}
        />
        {isAdmin ? (
          <TreeNode
            className="settings-node"
            id="admin-settings"
            key="admin"
            isLeaf={true}
            title={t("treeSettingsAdminSettings")}
          />
        ) : null}
        {enableThirdParty ? (
          <TreeNode
            selectable={true}
            className="settings-node"
            id="connected-clouds"
            key="thirdParty"
            isLeaf={true}
            title={t("treeSettingsConnectedCloud")}
          />
        ) : null}
      </TreeNode>
    );
  };

  const nodes = renderTreeNode();

  return (
    <StyledTreeMenu
      expandedKeys={expandedSetting}
      selectedKeys={selectedTreeNode}
      defaultExpandParent={false}
      disabled={isLoading}
      className="settings-tree-menu"
      switcherIcon={switcherIcon}
      onSelect={onSelect}
      showIcon={true}
      onExpand={onExpand}
    >
      {nodes}
    </StyledTreeMenu>
  );
};

const TreeSettingsContainer = withTranslation()(PureTreeSettings);

const TreeSettings = props => {
  useEffect(() => {
    changeLanguage(i18n);
  }, []);
  return (
    <I18nextProvider i18n={i18n}>
      <TreeSettingsContainer {...props} />
    </I18nextProvider>
  );
};

function mapStateToProps(state) {
  const { selectedTreeNode, settingsTree, isLoading } = state.files;

  const { isAdmin } = state.auth.user;

  const { expandedSetting, enableThirdParty } = settingsTree;

  return {
    selectedTreeNode,
    expandedSetting,
    enableThirdParty,
    isAdmin,
    isLoading
  };
}

export default connect(
  mapStateToProps,
  {
    setSelectedNode,
    setExpandSettingsTree
  }
)(withRouter(TreeSettings));
