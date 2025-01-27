import { makeObservable, observable, computed, action } from "mobx";
import { getFoldersTree } from "@appserver/common/api/files";
import { FolderType } from "@appserver/common/constants";
import selectedFolderStore from "./SelectedFolderStore";

class TreeFoldersStore {
  treeFolders = [];
  selectedTreeNode = [];
  expandedKeys = [];

  constructor() {
    makeObservable(this, {
      treeFolders: observable,
      selectedTreeNode: observable,
      expandedKeys: observable,

      myFolderId: computed,
      commonFolderId: computed,

      myFolder: computed,
      shareFolder: computed,
      favoritesFolder: computed,
      recentFolder: computed,
      privacyFolder: computed,
      commonFolder: computed,
      recycleBinFolder: computed,

      isMyFolder: computed,
      isShareFolder: computed,
      isFavoritesFolder: computed,
      isRecentFolder: computed,
      isPrivacyFolder: computed,
      isCommonFolder: computed,
      isRecycleBinFolder: computed,

      fetchTreeFolders: action,
      setTreeFolders: action,
      setExpandedKeys: action,
      setSelectedNode: action,
    });
  }

  fetchTreeFolders = async () => {
    const treeFolders = await getFoldersTree();
    this.setTreeFolders(treeFolders);
    return treeFolders;
  };

  setTreeFolders = (treeFolders) => {
    this.treeFolders = treeFolders;
  };

  setSelectedNode = (node) => {
    if (node[0]) {
      this.selectedTreeNode = node;
    }
  };

  setExpandedKeys = (expandedKeys) => {
    this.expandedKeys = expandedKeys;
  };

  addExpandedKeys = (item) => {
    this.expandedKeys.push(item);
  };

  get myFolder() {
    return this.treeFolders.find((x) => x.rootFolderName === "@my");
  }

  get shareFolder() {
    return this.treeFolders.find((x) => x.rootFolderName === "@share");
  }

  get favoritesFolder() {
    return this.treeFolders.find((x) => x.rootFolderName === "@favorites");
  }

  get recentFolder() {
    return this.treeFolders.find((x) => x.rootFolderName === "@recent");
  }

  get privacyFolder() {
    return this.treeFolders.find(
      (x) => x.rootFolderType === FolderType.Privacy
    );
  }

  get commonFolder() {
    return this.treeFolders.find((x) => x.rootFolderName === "@common");
  }

  get recycleBinFolder() {
    return this.treeFolders.find((x) => x.rootFolderName === "@trash");
  }

  get myFolderId() {
    return this.myFolder ? this.myFolder.id : null;
  }

  get commonFolderId() {
    return this.commonFolder ? this.commonFolder.id : null;
  }

  get isMyFolder() {
    return this.myFolder && this.myFolder.id === selectedFolderStore.id;
  }

  get isShareFolder() {
    return this.shareFolder && this.shareFolder.id === selectedFolderStore.id;
  }

  get isFavoritesFolder() {
    return (
      this.favoritesFolder && selectedFolderStore.id === this.favoritesFolder.id
    );
  }

  get isRecentFolder() {
    return this.recentFolder && selectedFolderStore.id === this.recentFolder.id;
  }

  get isPrivacyFolder() {
    return (
      this.privacyFolder &&
      this.privacyFolder.rootFolderType === selectedFolderStore.rootFolderType
    );
  }

  get isCommonFolder() {
    return this.commonFolder && this.commonFolder.id === selectedFolderStore.id;
  }

  get isRecycleBinFolder() {
    return (
      this.recycleBinFolder &&
      selectedFolderStore.id === this.recycleBinFolder.id
    );
  }

  get operationsFolders() {
    if (this.isPrivacyFolder) {
      return this.treeFolders.filter(
        (folder) => folder.rootFolderType === FolderType.Privacy && folder
      );
    } else {
      return this.treeFolders.filter(
        (folder) =>
          (folder.rootFolderType === FolderType.USER ||
            folder.rootFolderType === FolderType.COMMON ||
            folder.rootFolderType === FolderType.Projects) &&
          folder
      );
    }
  }
}

export default new TreeFoldersStore();
