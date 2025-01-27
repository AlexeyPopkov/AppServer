import React from "react";
import TreeMenu from "@appserver/components/tree-menu";
import TreeNode from "@appserver/components/tree-menu/sub-components/tree-node";
import styled from "styled-components";
//import equal from "fast-deep-equal/react";
import { getFolder } from "@appserver/common/api/files";
import { FolderType, ShareAccessRights } from "@appserver/common/constants";
import toastr from "studio/toastr";

import { onConvertFiles } from "../../../helpers/files-converter";
import { ReactSVG } from "react-svg";
import ExpanderDownIcon from "../../../../../../../public/images/expander-down.react.svg";
import ExpanderRightIcon from "../../../../../../../public/images/expander-right.react.svg";
import commonIconsStyles from "@appserver/components/utils/common-icons-style";

import { observer, inject } from "mobx-react";

const backgroundDragColor = "#EFEFB2";
const backgroundDragEnterColor = "#F8F7BF";

const StyledTreeMenu = styled(TreeMenu)`
  .rc-tree-node-content-wrapper {
    background: ${(props) => !props.dragging && "none !important"};
  }

  .rc-tree-node-selected {
    background: #dfe2e3 !important;
  }

  .rc-tree-treenode-disabled > span:not(.rc-tree-switcher),
  .rc-tree-treenode-disabled > a,
  .rc-tree-treenode-disabled > a span {
    cursor: wait;
  }
  /*
  span.rc-tree-iconEle {
    margin-left: 4px;
  }*/
`;
const StyledFolderSVG = styled.div`
  svg {
    width: 100%;

    path {
      fill: #657077;
    }
  }
`;
const StyledExpanderDownIcon = styled(ExpanderDownIcon)`
  ${commonIconsStyles}
  path {
    fill: dimgray;
  }
`;
const StyledExpanderRightIcon = styled(ExpanderRightIcon)`
  ${commonIconsStyles}
  path {
    fill: dimgray;
  }
`;
class TreeFolders extends React.Component {
  constructor(props) {
    super(props);

    this.state = { isExpand: false };
  }

  onBadgeClick = (e) => {
    const id = e.currentTarget.dataset.id;
    this.props.onBadgeClick && this.props.onBadgeClick(id);
  };

  getFolderIcon = (item) => {
    let iconUrl = "images/catalog.folder.react.svg";

    switch (item.rootFolderType) {
      case FolderType.USER:
        iconUrl = "images/catalog.user.react.svg";
        break;
      case FolderType.SHARE:
        iconUrl = "images/catalog.shared.react.svg";
        break;
      case FolderType.COMMON:
        iconUrl = "images/catalog.portfolio.react.svg";
        break;
      case FolderType.Favorites:
        iconUrl = "images/catalog.favorites.react.svg";
        break;
      case FolderType.Recent:
        iconUrl = "images/catalog.recent.react.svg";
        break;
      case FolderType.Privacy:
        iconUrl = "images/catalog.private.react.svg";
        break;
      case FolderType.TRASH:
        iconUrl = "/static/images/catalog.trash.react.svg";
        break;
      default:
        break;
    }

    if (item.parentId !== 0) iconUrl = "images/catalog.folder.react.svg";

    switch (item.providerKey) {
      case "GoogleDrive":
        iconUrl = "images/cloud.services.google.drive.react.svg";
        break;
      case "Box":
        iconUrl = "images/cloud.services.box.react.svg";
        break;
      case "DropboxV2":
        iconUrl = "images/cloud.services.dropbox.react.svg";
        break;
      case "OneDrive":
        iconUrl = "images/cloud.services.onedrive.react.svg";
        break;
      case "SharePoint":
        iconUrl = "images/cloud.services.onedrive.react.svg";
        break;
      case "kDrive":
        iconUrl = "images/catalog.folder.react.svg";
        break;
      case "Yandex":
        iconUrl = "images/catalog.folder.react.svg";
        break;
      case "NextCloud":
        iconUrl = "images/cloud.services.nextcloud.react.svg";
        break;
      case "OwnCloud":
        iconUrl = "images/catalog.folder.react.svg";
        break;
      case "WebDav":
        iconUrl = "images/catalog.folder.react.svg";
        break;
      default:
        break;
    }

    return (
      <StyledFolderSVG>
        <ReactSVG src={iconUrl} />
      </StyledFolderSVG>
    );
  };

  showDragItems = (item) => {
    const {
      isAdmin,
      myId,
      commonId,
      rootFolderId,
      currentId,
      draggableItems,
    } = this.props;
    if (item.id === currentId) {
      return false;
    }

    if (draggableItems.find((x) => x.id === item.id)) return false;

    const isMy = rootFolderId === FolderType.USER;
    const isCommon = rootFolderId === FolderType.COMMON;
    const isShare = rootFolderId === FolderType.SHARE;

    if (
      item.rootFolderType === FolderType.SHARE &&
      item.access === ShareAccessRights.FullAccess
    ) {
      return true;
    }

    if (isAdmin) {
      if (isMy || isCommon || isShare) {
        if (
          (item.pathParts &&
            (item.pathParts[0] === myId || item.pathParts[0] === commonId)) ||
          item.rootFolderType === FolderType.USER ||
          item.rootFolderType === FolderType.COMMON
        ) {
          return true;
        }
      }
    } else {
      if (isMy || isCommon || isShare) {
        if (
          (item.pathParts && item.pathParts[0] === myId) ||
          item.rootFolderType === FolderType.USER
        ) {
          return true;
        }
      }
    }

    return false;
  };

  getItems = (data) => {
    return data.map((item) => {
      const dragging = this.props.dragging ? this.showDragItems(item) : false;

      const showBadge = item.newItems
        ? item.newItems > 0 && this.props.needUpdate
        : false;

      const serviceFolder = !!item.providerKey;
      if ((item.folders && item.folders.length > 0) || serviceFolder) {
        return (
          <TreeNode
            id={item.id}
            key={item.id}
            title={item.title}
            needTopMargin={item.rootFolderType === FolderType.Privacy}
            icon={this.getFolderIcon(item)}
            dragging={dragging}
            isLeaf={
              item.rootFolderType === FolderType.Privacy &&
              !this.props.isDesktop
                ? true
                : null
            }
            newItems={
              !this.props.isDesktop &&
              item.rootFolderType === FolderType.Privacy
                ? null
                : item.newItems
            }
            providerKey={item.providerKey}
            onBadgeClick={this.onBadgeClick}
            showBadge={showBadge}
          >
            {item.rootFolderType === FolderType.Privacy && !this.props.isDesktop
              ? null
              : this.getItems(item.folders ? item.folders : [])}
          </TreeNode>
        );
      }
      return (
        <TreeNode
          id={item.id}
          key={item.id}
          title={item.title}
          needTopMargin={item.rootFolderType === FolderType.TRASH}
          dragging={dragging}
          isLeaf={item.foldersCount ? false : true}
          icon={this.getFolderIcon(item)}
          newItems={
            !this.props.isDesktop && item.rootFolderType === FolderType.Privacy
              ? null
              : item.newItems
          }
          providerKey={item.providerKey}
          onBadgeClick={this.onBadgeClick}
          showBadge={showBadge}
        />
      );
    });
  };

  switcherIcon = (obj) => {
    if (obj.isLeaf) {
      return null;
    }
    if (obj.expanded) {
      return <StyledExpanderDownIcon size="scale" />;
    } else {
      return <StyledExpanderRightIcon size="scale" />;
    }
  };

  loop = (data, curId, child, level) => {
    //if (level < 1 || curId.length - 3 > level * 2) return;
    data.forEach((item) => {
      const itemId = item.id.toString();
      if (curId.indexOf(itemId) >= 0) {
        const listIds = curId;
        const treeItem = listIds.find((x) => x.toString() === itemId);
        if (treeItem === undefined) {
          listIds.push(itemId);
        }
        if (item.folders) {
          this.loop(item.folders, listIds, child);
        } else {
          item.folders = child;
        }
      }
    });
  };

  getNewTreeData(treeData, curId, child, level) {
    this.loop(treeData, curId, child, level);
    this.setLeaf(treeData, curId, level);
  }

  setLeaf(treeData, curKey, level) {
    const loopLeaf = (data, lev) => {
      const l = lev - 1;
      data.forEach((item) => {
        if (
          item.key.length > curKey.length
            ? item.key.indexOf(curKey) !== 0
            : curKey.indexOf(item.key) !== 0
        ) {
          return;
        }
        if (item.folders) {
          loopLeaf(item.folders, l);
        } else if (l < 1) {
          item.isLeaf = true;
        }
      });
    };
    loopLeaf(treeData, level + 1);
  }

  generateTreeNodes = (treeNode) => {
    const folderId = treeNode.props.id;
    let arrayFolders;

    const newFilter = this.props.filter.clone();
    newFilter.filterType = 2;
    newFilter.withSubfolders = null;
    newFilter.authorType = null;

    return getFolder(folderId, newFilter)
      .then((data) => {
        arrayFolders = data.folders;

        let listIds = [];
        for (let item of data.pathParts) {
          listIds.push(item.toString());
        }

        const folderIndex = treeNode.props.pos;
        let i = 0;
        for (let item of arrayFolders) {
          item["key"] = `${folderIndex}-${i}`;
          i++;
        }

        return { folders: arrayFolders, listIds };
      })
      .catch((err) => toastr.error("Something went wrong", err));
  };

  onLoadData = (treeNode, isExpand) => {
    isExpand && this.setState({ isExpand: true });
    this.props.setIsLoading && this.props.setIsLoading(true);
    //console.log("load data...", treeNode);

    if (this.state.isExpand && !isExpand) {
      return Promise.resolve();
    }

    return this.generateTreeNodes(treeNode)
      .then((data) => {
        const itemId = treeNode.props.id.toString();
        const listIds = data.listIds;
        listIds.push(itemId);

        const treeData = [...this.props.treeFolders];

        this.getNewTreeData(treeData, listIds, data.folders, 10);
        this.props.needUpdate && this.props.setTreeFolders(treeData);
        //this.setState({ treeData });
      })
      .catch((err) => toastr.error(err))
      .finally(() => {
        this.setState({ isExpand: false });
        this.props.setIsLoading && this.props.setIsLoading(false);
      });
  };

  onExpand = (data, treeNode) => {
    if (treeNode.node && !treeNode.node.props.children) {
      if (treeNode.expanded) {
        this.onLoadData(treeNode.node, true);
      }
    }
    if (this.props.needUpdate) {
      const expandedKeys = data;
      this.props.setExpandedKeys(expandedKeys);
    }
  };

  onMouseEnter = (data) => {
    if (this.props.dragging) {
      if (data.node.props.dragging) {
        this.props.setDragItem(data.node.props.id);
      }
    }
  };

  onMouseLeave = () => {
    if (this.props.dragging) {
      this.props.setDragItem(null);
    }
  };

  onDragOver = (data) => {
    const parentElement = data.event.target.parentElement;
    const existElement = parentElement.classList.contains(
      "rc-tree-node-content-wrapper"
    );

    if (existElement) {
      if (data.node.props.dragging) {
        parentElement.style.background = backgroundDragColor;
      }
    }
  };

  onDragLeave = (data) => {
    const parentElement = data.event.target.parentElement;
    const existElement = parentElement.classList.contains(
      "rc-tree-node-content-wrapper"
    );

    if (existElement) {
      if (data.node.props.dragging) {
        parentElement.style.background = backgroundDragEnterColor;
      }
    }
  };

  onDrop = (data) => {
    const { setDragging, onTreeDrop } = this.props;
    const { dragging, id } = data.node.props;
    setDragging(false);
    if (dragging) {
      const promise = new Promise((resolve) =>
        onConvertFiles(data.event, resolve)
      );
      promise.then((files) => onTreeDrop(files, id));
    }
  };

  render() {
    const {
      selectedKeys,
      isLoading,
      onSelect,
      dragging,
      expandedKeys,
      treeFolders,
    } = this.props;
    //const loadProp = needUpdate ? { loadData: this.onLoadData } : {};

    return (
      <StyledTreeMenu
        className="files-tree-menu"
        checkable={false}
        draggable
        disabled={isLoading}
        multiple={false}
        showIcon
        switcherIcon={this.switcherIcon}
        onSelect={onSelect}
        selectedKeys={selectedKeys}
        //{...loadProp}
        loadData={this.onLoadData}
        expandedKeys={expandedKeys}
        onExpand={this.onExpand}
        onMouseEnter={this.onMouseEnter}
        onMouseLeave={this.onMouseLeave}
        onDragOver={this.onDragOver}
        onDragLeave={this.onDragLeave}
        onDrop={this.onDrop}
        dragging={dragging}
        gapBetweenNodes="22"
        gapBetweenNodesTablet="26"
        isFullFillSelection={false}
      >
        {this.getItems(treeFolders)}
      </StyledTreeMenu>
    );
  }
}

TreeFolders.defaultProps = {
  selectedKeys: [],
  needUpdate: true,
};

export default inject(
  ({
    auth,
    initFilesStore,
    filesStore,
    treeFoldersStore,
    selectedFolderStore,
  }) => {
    const { setIsLoading, dragging, setDragging, setDragItem } = initFilesStore;
    const { filter, setFilter, selection } = filesStore;

    const {
      treeFolders,
      setTreeFolders,
      myFolderId,
      commonFolderId,
      isPrivacyFolder,
      expandedKeys,
      setExpandedKeys,
    } = treeFoldersStore;
    const { pathParts, id } = selectedFolderStore;

    return {
      isAdmin: auth.isAdmin,
      isDesktop: auth.settingsStore.isDesktopClient,
      dragging,
      rootFolderId: pathParts,
      currentId: id,
      myId: myFolderId,
      commonId: commonFolderId,
      isPrivacy: isPrivacyFolder,
      filter,
      draggableItems: dragging ? selection : false,
      expandedKeys,
      treeFolders,

      setDragging,
      setIsLoading,
      setTreeFolders,
      setFilter,
      setDragItem,
      setExpandedKeys,
    };
  }
)(observer(TreeFolders));
