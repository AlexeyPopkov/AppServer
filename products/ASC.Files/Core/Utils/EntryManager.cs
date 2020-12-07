/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ASC.Common;
using ASC.Common.Caching;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Common.Settings;
using ASC.Core.Users;
using ASC.Files.Core;
using ASC.Files.Core.Resources;
using ASC.Files.Core.Security;
using ASC.Web.Core.Files;
using ASC.Web.Files.Api;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Core;
using ASC.Web.Files.Helpers;
using ASC.Web.Files.Services.DocumentService;
using ASC.Web.Files.ThirdPartyApp;
using ASC.Web.Studio.Core;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Newtonsoft.Json.Linq;

using FileShare = ASC.Files.Core.Security.FileShare;

namespace ASC.Web.Files.Utils
{
    [Scope]
    public class LockerManager
    {
        private AuthContext AuthContext { get; }
        private IDaoFactory DaoFactory { get; }

        public LockerManager(AuthContext authContext, IDaoFactory daoFactory)
        {
            AuthContext = authContext;
            DaoFactory = daoFactory;
        }

        public bool FileLockedForMe<T>(T fileId, Guid userId = default)
        {
            var app = ThirdPartySelector.GetAppByFileId(fileId.ToString());
            if (app != null)
            {
                return false;
            }

            userId = userId == default ? AuthContext.CurrentAccount.ID : userId;
            var tagDao = DaoFactory.GetTagDao<T>();
            var lockedBy = FileLockedBy(fileId, tagDao);
            return lockedBy != Guid.Empty && lockedBy != userId;
        }

        public Guid FileLockedBy<T>(T fileId, ITagDao<T> tagDao)
        {
            var tagLock = tagDao.GetTags(fileId, FileEntryType.File, TagType.Locked).FirstOrDefault();
            return tagLock != null ? tagLock.Owner : Guid.Empty;
        }
    }

    [Scope]
    public class BreadCrumbsManager
    {
        private IDaoFactory DaoFactory { get; }
        private FileSecurity FileSecurity { get; }
        private GlobalFolderHelper GlobalFolderHelper { get; }
        private AuthContext AuthContext { get; }

        public BreadCrumbsManager(
            IDaoFactory daoFactory,
            FileSecurity fileSecurity,
            GlobalFolderHelper globalFolderHelper,
            AuthContext authContext)
        {
            DaoFactory = daoFactory;
            FileSecurity = fileSecurity;
            GlobalFolderHelper = globalFolderHelper;
            AuthContext = authContext;
        }

        public async Task<List<FileEntry>> GetBreadCrumbs<T>(T folderId)
        {
            var folderDao = DaoFactory.GetFolderDao<T>();
            return await GetBreadCrumbs(folderId, folderDao);
        }

        public async Task<List<FileEntry>> GetBreadCrumbs<T>(T folderId, IFolderDao<T> folderDao)
        {
            if (folderId == null) return new List<FileEntry>();
            var breadCrumbs = (await FileSecurity.FilterRead(await folderDao.GetParentFolders(folderId))).Cast<FileEntry>().ToList();

            var firstVisible = breadCrumbs.ElementAtOrDefault(0) as Folder<T>;

            var rootId = 0;
            if (firstVisible == null)
            {
                rootId = await GlobalFolderHelper.FolderShare;
            }
            else
            {
                switch (firstVisible.FolderType)
                {
                    case FolderType.DEFAULT:
                        if (!firstVisible.ProviderEntry)
                        {
                            rootId = await GlobalFolderHelper.FolderShare;
                        }
                        else
                        {
                            switch (firstVisible.RootFolderType)
                            {
                                case FolderType.USER:
                                    rootId = AuthContext.CurrentAccount.ID == firstVisible.RootFolderCreator
                                        ? await GlobalFolderHelper.FolderMy
                                        : await GlobalFolderHelper.FolderShare;
                                    break;
                                case FolderType.COMMON:
                                    rootId = await GlobalFolderHelper.FolderCommon;
                                    break;
                            }
                        }
                        break;

                    case FolderType.BUNCH:
                        rootId = await GlobalFolderHelper.FolderProjects;
                        break;
                }
            }

            var folderDaoInt = DaoFactory.GetFolderDao<int>();

            if (rootId != 0)
            {
                breadCrumbs.Insert(0, await folderDaoInt.GetFolder(rootId));
            }

            return breadCrumbs;
        }
    }

    [Scope]
    public class EntryManager
    {
        private const string UPDATE_LIST = "filesUpdateList";
        private readonly ICache cache;

        private IDaoFactory DaoFactory { get; }
        private FileSecurity FileSecurity { get; }
        private GlobalFolderHelper GlobalFolderHelper { get; }
        private PathProvider PathProvider { get; }
        private AuthContext AuthContext { get; }
        private FilesIntegration FilesIntegration { get; }
        private FileMarker FileMarker { get; }
        private FileUtility FileUtility { get; }
        private Global Global { get; }
        private GlobalStore GlobalStore { get; }
        private CoreBaseSettings CoreBaseSettings { get; }
        private FilesSettingsHelper FilesSettingsHelper { get; }
        private UserManager UserManager { get; }
        private FileShareLink FileShareLink { get; }
        private DocumentServiceHelper DocumentServiceHelper { get; }
        private ThirdpartyConfiguration ThirdpartyConfiguration { get; }
        private DocumentServiceConnector DocumentServiceConnector { get; }
        private LockerManager LockerManager { get; }
        private BreadCrumbsManager BreadCrumbsManager { get; }
        public TenantManager TenantManager { get; }
        public SettingsManager SettingsManager { get; }
        private IServiceProvider ServiceProvider { get; }
        public ILog Logger { get; }

        public EntryManager(
            IDaoFactory daoFactory,
            FileSecurity fileSecurity,
            GlobalFolderHelper globalFolderHelper,
            PathProvider pathProvider,
            AuthContext authContext,
            FilesIntegration filesIntegration,
            FileMarker fileMarker,
            FileUtility fileUtility,
            Global global,
            GlobalStore globalStore,
            CoreBaseSettings coreBaseSettings,
            FilesSettingsHelper filesSettingsHelper,
            UserManager userManager,
            IOptionsMonitor<ILog> optionsMonitor,
            FileShareLink fileShareLink,
            DocumentServiceHelper documentServiceHelper,
            ThirdpartyConfiguration thirdpartyConfiguration,
            DocumentServiceConnector documentServiceConnector,
            LockerManager lockerManager,
            BreadCrumbsManager breadCrumbsManager,
            TenantManager tenantManager,
            SettingsManager settingsManager,
            IServiceProvider serviceProvider)
        {
            DaoFactory = daoFactory;
            FileSecurity = fileSecurity;
            GlobalFolderHelper = globalFolderHelper;
            PathProvider = pathProvider;
            AuthContext = authContext;
            FilesIntegration = filesIntegration;
            FileMarker = fileMarker;
            FileUtility = fileUtility;
            Global = global;
            GlobalStore = globalStore;
            CoreBaseSettings = coreBaseSettings;
            FilesSettingsHelper = filesSettingsHelper;
            UserManager = userManager;
            FileShareLink = fileShareLink;
            DocumentServiceHelper = documentServiceHelper;
            ThirdpartyConfiguration = thirdpartyConfiguration;
            DocumentServiceConnector = documentServiceConnector;
            LockerManager = lockerManager;
            BreadCrumbsManager = breadCrumbsManager;
            TenantManager = tenantManager;
            SettingsManager = settingsManager;
            ServiceProvider = serviceProvider;
            Logger = optionsMonitor.CurrentValue;
            cache = AscCache.Memory;
        }

        public async Task<(IEnumerable<FileEntry>, int)> GetEntries<T>(Folder<T> parent, int from, int count, FilterType filter, bool subjectGroup, Guid subjectId, string searchText, bool searchInContent, bool withSubfolders, OrderBy orderBy)
        {
            var total = 0;

            if (parent == null) throw new ArgumentNullException("parent", FilesCommonResource.ErrorMassage_FolderNotFound);
            if (parent.ProviderEntry && !FilesSettingsHelper.EnableThirdParty) throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_ReadFolder);
            if (parent.RootFolderType == FolderType.Privacy && (!PrivacyRoomSettings.IsAvailable(TenantManager) || !PrivacyRoomSettings.GetEnabled(SettingsManager))) throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_ReadFolder);

            var fileSecurity = FileSecurity;
            var entries = Enumerable.Empty<FileEntry>();

            searchInContent = searchInContent && filter != FilterType.ByExtension && !Equals(parent.ID, GlobalFolderHelper.FolderTrash);

            if (parent.FolderType == FolderType.Projects && parent.ID.Equals(await GlobalFolderHelper.FolderProjects))
            {
                //TODO
                //var apiServer = new ASC.Api.ApiServer();
                //var apiUrl = string.Format("{0}project/maxlastmodified.json", SetupInfo.WebApiBaseUrl);

                string responseBody = null;// apiServer.GetApiResponse(apiUrl, "GET");
                if (responseBody != null)
                {
                    var responseApi = JObject.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(responseBody)));

                    var projectLastModified = responseApi["response"].Value<string>();
                    var projectListCacheKey = string.Format("documents/projectFolders/{0}", AuthContext.CurrentAccount.ID);
                    Dictionary<int, KeyValuePair<int, string>> folderIDProjectTitle = null;

                    if (folderIDProjectTitle == null)
                    {
                        //apiUrl = string.Format("{0}project/filter.json?sortBy=title&sortOrder=ascending&status=open&fields=id,title,security,projectFolder", SetupInfo.WebApiBaseUrl);

                        responseApi = JObject.Parse(""); //Encoding.UTF8.GetString(Convert.FromBase64String(apiServer.GetApiResponse(apiUrl, "GET"))));

                        var responseData = responseApi["response"];

                        if (!(responseData is JArray)) return (entries.ToList(), total);

                        folderIDProjectTitle = new Dictionary<int, KeyValuePair<int, string>>();
                        foreach (JObject projectInfo in responseData.Children())
                        {
                            var projectID = projectInfo["id"].Value<int>();
                            var projectTitle = Global.ReplaceInvalidCharsAndTruncate(projectInfo["title"].Value<string>());

                            if (projectInfo.TryGetValue("security", out var projectSecurityJToken))
                            {
                                var projectSecurity = projectInfo["security"].Value<JObject>();
                                if (projectSecurity.TryGetValue("canReadFiles", out var projectCanFileReadJToken))
                                {
                                    if (!projectSecurity["canReadFiles"].Value<bool>())
                                    {
                                        continue;
                                    }
                                }
                            }

                            int projectFolderID;
                            if (projectInfo.TryGetValue("projectFolder", out var projectFolderIDjToken))
                                projectFolderID = projectFolderIDjToken.Value<int>();
                            else
                                projectFolderID = await FilesIntegration.RegisterBunch<int>("projects", "project", projectID.ToString());

                            if (!folderIDProjectTitle.ContainsKey(projectFolderID))
                                folderIDProjectTitle.Add(projectFolderID, new KeyValuePair<int, string>(projectID, projectTitle));

                            AscCache.Memory.Remove("documents/folders/" + projectFolderID);
                            AscCache.Memory.Insert("documents/folders/" + projectFolderID, projectTitle, TimeSpan.FromMinutes(30));
                        }
                    }

                    var rootKeys = folderIDProjectTitle.Keys.ToArray();
                    if (filter == FilterType.None || filter == FilterType.FoldersOnly)
                    {
                        var folders = await DaoFactory.GetFolderDao<int>().GetFolders(rootKeys, filter, subjectGroup, subjectId, searchText, withSubfolders, false);

                        var emptyFilter = string.IsNullOrEmpty(searchText) && filter == FilterType.None && subjectId == Guid.Empty;
                        if (!emptyFilter)
                        {
                            var projectFolderIds =
                                folderIDProjectTitle
                                    .Where(projectFolder => string.IsNullOrEmpty(searchText)
                                                            || (projectFolder.Value.Value ?? "").ToLower().Trim().Contains(searchText.ToLower().Trim()))
                                    .Select(projectFolder => projectFolder.Key);

                            folders.RemoveAll(folder => rootKeys.Contains(folder.ID));

                            var projectFolders = await DaoFactory.GetFolderDao<int>().GetFolders(projectFolderIds.ToList(), filter, subjectGroup, subjectId, null, false, false);
                            folders.AddRange(projectFolders);
                        }

                        folders.ForEach(async x =>
                            {
                                x.Title = folderIDProjectTitle.ContainsKey(x.ID) ? folderIDProjectTitle[x.ID].Value : x.Title;
                                x.FolderUrl = folderIDProjectTitle.ContainsKey(x.ID) ? await PathProvider.GetFolderUrl(x, folderIDProjectTitle[x.ID].Key) : string.Empty;
                            });

                        if (withSubfolders)
                        {
                            folders = (await fileSecurity.FilterRead(folders)).ToList();
                        }

                        entries = entries.Concat(folders.Cast<FileEntry<T>>());
                    }

                    if (filter != FilterType.FoldersOnly && withSubfolders)
                    {
                        var files = (await DaoFactory.GetFileDao<int>().GetFiles(rootKeys, filter, subjectGroup, subjectId, searchText, searchInContent)).ToList();
                        files = (await fileSecurity.FilterRead(files)).ToList();
                        entries = entries.Concat(files.Cast<FileEntry<T>>());
                    }
                }

                parent.TotalFiles = entries.Aggregate(0, (a, f) => a + (f.FileEntryType == FileEntryType.Folder ? ((IFolder)f).TotalFiles : 1));
                parent.TotalSubFolders = entries.Aggregate(0, (a, f) => a + (f.FileEntryType == FileEntryType.Folder ? ((IFolder)f).TotalSubFolders + 1 : 0));
            }
            else if (parent.FolderType == FolderType.SHARE)
            {
                //share
                var shared = await fileSecurity.GetSharesForMe(filter, subjectGroup, subjectId, searchText, searchInContent, withSubfolders);

                entries = entries.Concat(shared);

                parent.TotalFiles = entries.Aggregate(0, (a, f) => a + (f.FileEntryType == FileEntryType.Folder ? ((IFolder)f).TotalFiles : 1));
                parent.TotalSubFolders = entries.Aggregate(0, (a, f) => a + (f.FileEntryType == FileEntryType.Folder ? ((IFolder)f).TotalSubFolders + 1 : 0));
            }
            else if (parent.FolderType == FolderType.Recent)
            {
                var fileDao = DaoFactory.GetFileDao<T>();
                var files = await GetRecent(fileDao, filter, subjectGroup, subjectId, searchText, searchInContent);
                entries = entries.Concat(files);

                parent.TotalFiles = entries.Aggregate(0, (a, f) => a + (f.FileEntryType == FileEntryType.Folder ? ((IFolder)f).TotalFiles : 1));
            }
            else if (parent.FolderType == FolderType.Favorites)
            {
                var fileDao = DaoFactory.GetFileDao<T>();
                var folderDao = DaoFactory.GetFolderDao<T>();

                var (folders, files) = await GetFavorites(folderDao, fileDao, filter, subjectGroup, subjectId, searchText, searchInContent);

                entries = entries.Concat(folders);
                entries = entries.Concat(files);

                parent.TotalFiles = entries.Aggregate(0, (a, f) => a + (f.FileEntryType == FileEntryType.Folder ? ((IFolder)f).TotalFiles : 1));
                parent.TotalSubFolders = entries.Aggregate(0, (a, f) => a + (f.FileEntryType == FileEntryType.Folder ? ((IFolder)f).TotalSubFolders + 1 : 0));
            }
            else if (parent.FolderType == FolderType.Templates)
            {
                var fileDao = DaoFactory.GetFileDao<T>();
                var files = await GetTemplates(fileDao, filter, subjectGroup, subjectId, searchText, searchInContent);
                entries = entries.Concat(files);

                parent.TotalFiles = entries.Aggregate(0, (a, f) => a + (f.FileEntryType == FileEntryType.Folder ? ((IFolder)f).TotalFiles : 1));
                parent.TotalSubFolders = 0;
            }
            else if (parent.FolderType == FolderType.Privacy)
            {
                var folderDao = DaoFactory.GetFolderDao<T>();
                var fileDao = DaoFactory.GetFileDao<T>();
                var folders = (await folderDao.GetFolders(parent.ID, orderBy, filter, subjectGroup, subjectId, searchText, withSubfolders)).Cast<Folder<T>>();
                folders = await fileSecurity.FilterRead(folders);
                entries = entries.Concat(folders);

                var files = (await fileDao.GetFiles(parent.ID, orderBy, filter, subjectGroup, subjectId, searchText, searchInContent, withSubfolders)).Cast<File<T>>();
                files = await fileSecurity.FilterRead(files);
                entries = entries.Concat(files);

                //share
                var shared = await fileSecurity.GetPrivacyForMe(filter, subjectGroup, subjectId, searchText, searchInContent, withSubfolders);

                entries = entries.Concat(shared);

                parent.TotalFiles = entries.Aggregate(0, (a, f) => a + (f.FileEntryType == FileEntryType.Folder ? ((IFolder)f).TotalFiles : 1));
                parent.TotalSubFolders = entries.Aggregate(0, (a, f) => a + (f.FileEntryType == FileEntryType.Folder ? ((IFolder)f).TotalSubFolders + 1 : 0));
            }
            else
            {
                if (parent.FolderType == FolderType.TRASH)
                    withSubfolders = false;

                var folders = await DaoFactory.GetFolderDao<T>().GetFolders(parent.ID, orderBy, filter, subjectGroup, subjectId, searchText, withSubfolders);
                folders = (await fileSecurity.FilterRead(folders)).ToList();
                entries = entries.Concat(folders);


                var files = await DaoFactory.GetFileDao<T>()
                    .GetFiles(parent.ID, orderBy, filter, subjectGroup, subjectId, searchText, searchInContent, withSubfolders);
                entries = entries.Concat(await fileSecurity.FilterRead(files));

                if (filter == FilterType.None || filter == FilterType.FoldersOnly)
                {
                    var folderList = await GetThirpartyFolders(parent, searchText);

                    var thirdPartyFolder = FilterEntries(folderList, filter, subjectGroup, subjectId, searchText, searchInContent);
                    entries = entries.Concat(thirdPartyFolder);
                }
            }

            if (orderBy.SortedBy != SortedByType.New && parent.FolderType != FolderType.Recent)
            {
                entries = SortEntries<T>(entries, orderBy);

                total = entries.Count();
                if (0 < from) entries = entries.Skip(from);
                if (0 < count) entries = entries.Take(count);
            }

            entries = await FileMarker.SetTagsNew(parent, entries);

            //sorting after marking
            if (orderBy.SortedBy == SortedByType.New)
            {
                entries = SortEntries<T>(entries, orderBy);

                total = entries.Count();
                if (0 < from) entries = entries.Skip(from);
                if (0 < count) entries = entries.Take(count);
            }

            SetFileStatus(entries.OfType<File<T>>().Where(r => r != null && r.ID != null && r.FileEntryType == FileEntryType.File).ToList());
            return (entries, total);
        }

        public async Task<IEnumerable<File<T>>> GetTemplates<T>(IFileDao<T> fileDao, FilterType filter, bool subjectGroup, Guid subjectId, string searchText, bool searchInContent)
        {
            var tagDao = DaoFactory.GetTagDao<T>();
            var tags = tagDao.GetTags(AuthContext.CurrentAccount.ID, TagType.Template);

            var fileIds = tags.Where(tag => tag.EntryType == FileEntryType.File).Select(tag => (T)Convert.ChangeType(tag.EntryId, typeof(T))).ToArray();

            var files = await fileDao.GetFilesFiltered(fileIds, filter, subjectGroup, subjectId, searchText, searchInContent);
            files = files.Where(file => file.RootFolderType != FolderType.TRASH).ToList();

            files = (await FileSecurity.FilterRead(files)).ToList();

            return files;
        }

        public async Task<IEnumerable<Folder<string>>> GetThirpartyFolders<T>(Folder<T> parent, string searchText = null)
        {
            var folderList = new List<Folder<string>>();

            if ((parent.ID.Equals(await GlobalFolderHelper.FolderMy) || parent.ID.Equals(await GlobalFolderHelper.FolderCommon))
                && ThirdpartyConfiguration.SupportInclusion(DaoFactory)
                && (FilesSettingsHelper.EnableThirdParty
                    || CoreBaseSettings.Personal))
            {
                var providerDao = DaoFactory.ProviderDao;
                if (providerDao == null) return folderList;

                var fileSecurity = FileSecurity;

                var providers = providerDao.GetProvidersInfo(parent.RootFolderType, searchText);
                folderList = providers
                    .Select(providerInfo => GetFakeThirdpartyFolder<T>(providerInfo, parent.ID.ToString()))
                    .Where(r => fileSecurity.CanRead(r).Result).ToList(); //TODO: Result

                if (folderList.Any())
                {
                    var securityDao = DaoFactory.GetSecurityDao<string>();
                    securityDao.GetPureShareRecords(folderList)
                    //.Where(x => x.Owner == SecurityContext.CurrentAccount.ID)
                    .Select(x => x.EntryId).Distinct().ToList()
                    .ForEach(id =>
                    {
                        folderList.First(y => y.ID.Equals(id)).Shared = true;
                    });
                }
            }

            return folderList;
        }

        public async Task<IEnumerable<File<T>>> GetRecent<T>(IFileDao<T> fileDao, FilterType filter, bool subjectGroup, Guid subjectId, string searchText, bool searchInContent)
        {
            var tagDao = DaoFactory.GetTagDao<T>();
            var tags = tagDao.GetTags(AuthContext.CurrentAccount.ID, TagType.Recent).ToList();

            var fileIds = tags.Where(tag => tag.EntryType == FileEntryType.File).Select(tag => (T)Convert.ChangeType(tag.EntryId, typeof(T))).ToArray();
            var files = await fileDao.GetFilesFiltered(fileIds, filter, subjectGroup, subjectId, searchText, searchInContent);
            files = files.Where(file => file.RootFolderType != FolderType.TRASH).ToList();

            files = (await FileSecurity.FilterRead(files)).ToList();

            var listFileIds = fileIds.ToList();
            files = files.OrderBy(file => listFileIds.IndexOf(file.ID)).ToList();

            return files;
        }

        public async Task<(IEnumerable<Folder<T>>, IEnumerable<File<T>>)> GetFavorites<T>(IFolderDao<T> folderDao, IFileDao<T> fileDao, FilterType filter, bool subjectGroup, Guid subjectId, string searchText, bool searchInContent)
        {
            var folders = new List<Folder<T>>();
            var files = new List<File<T>>();
            var fileSecurity = FileSecurity;
            var tagDao = DaoFactory.GetTagDao<T>();
            var tags = tagDao.GetTags(AuthContext.CurrentAccount.ID, TagType.Favorite);

            if (filter == FilterType.None || filter == FilterType.FoldersOnly)
            {
                var folderIds = tags.Where(tag => tag.EntryType == FileEntryType.Folder).Select(tag => (T)Convert.ChangeType(tag.EntryId, typeof(T))).ToList();
                folders = await folderDao.GetFolders(folderIds, filter, subjectGroup, subjectId, searchText, false, false);
                folders = folders.Where(folder => folder.RootFolderType != FolderType.TRASH).ToList();

                folders = (await fileSecurity.FilterRead(folders)).ToList();
            }

            if (filter != FilterType.FoldersOnly)
            {
                var fileIds = tags.Where(tag => tag.EntryType == FileEntryType.File).Select(tag => (T)Convert.ChangeType(tag.EntryId, typeof(T))).ToArray();
                files = await fileDao.GetFilesFiltered(fileIds, filter, subjectGroup, subjectId, searchText, searchInContent);
                files = files.Where(file => file.RootFolderType != FolderType.TRASH).ToList();

                files = (await fileSecurity.FilterRead(files)).ToList();
            }

            return (folders, files);
        }

        public IEnumerable<FileEntry<T>> FilterEntries<T>(IEnumerable<FileEntry<T>> entries, FilterType filter, bool subjectGroup, Guid subjectId, string searchText, bool searchInContent)
        {
            if (entries == null || !entries.Any()) return entries;

            if (subjectId != Guid.Empty)
            {
                entries = entries.Where(f =>
                                        subjectGroup
                                            ? UserManager.GetUsersByGroup(subjectId).Any(s => s.ID == f.CreateBy)
                                            : f.CreateBy == subjectId
                    )
                                 .ToList();
            }

            Func<FileEntry<T>, bool> where = null;

            switch (filter)
            {
                case FilterType.SpreadsheetsOnly:
                case FilterType.PresentationsOnly:
                case FilterType.ImagesOnly:
                case FilterType.DocumentsOnly:
                case FilterType.ArchiveOnly:
                case FilterType.FilesOnly:
                case FilterType.MediaOnly:
                    where = f => f.FileEntryType == FileEntryType.File && (((File<T>)f).FilterType == filter || filter == FilterType.FilesOnly);
                    break;
                case FilterType.FoldersOnly:
                    where = f => f.FileEntryType == FileEntryType.Folder;
                    break;
                case FilterType.ByExtension:
                    var filterExt = (searchText ?? string.Empty).ToLower().Trim();
                    where = f => !string.IsNullOrEmpty(filterExt) && f.FileEntryType == FileEntryType.File && FileUtility.GetFileExtension(f.Title).Contains(filterExt);
                    break;
            }

            if (where != null)
            {
                entries = entries.Where(where).ToList();
            }

            if ((!searchInContent || filter == FilterType.ByExtension) && !string.IsNullOrEmpty(searchText = (searchText ?? string.Empty).ToLower().Trim()))
            {
                entries = entries.Where(f => f.Title.ToLower().Contains(searchText)).ToList();
            }
            return entries;
        }

        public IEnumerable<FileEntry> SortEntries<T>(IEnumerable<FileEntry> entries, OrderBy orderBy)
        {
            if (entries == null || !entries.Any()) return entries;

            if (orderBy == null)
            {
                orderBy = FilesSettingsHelper.DefaultOrder;
            }

            var c = orderBy.IsAsc ? 1 : -1;
            Comparison<FileEntry> sorter = orderBy.SortedBy switch
            {
                SortedByType.Type => (x, y) =>
                             {
                                 var cmp = 0;
                                 if (x.FileEntryType == FileEntryType.File && y.FileEntryType == FileEntryType.File)
                                     cmp = c * (FileUtility.GetFileExtension((x.Title)).CompareTo(FileUtility.GetFileExtension(y.Title)));
                                 return cmp == 0 ? x.Title.EnumerableComparer(y.Title) : cmp;
                }
                ,
                SortedByType.Author => (x, y) =>
                             {
                                 var cmp = c * string.Compare(x.ModifiedByString, y.ModifiedByString);
                                 return cmp == 0 ? x.Title.EnumerableComparer(y.Title) : cmp;
                }
                ,
                SortedByType.Size => (x, y) =>
                             {
                                 var cmp = 0;
                                 if (x.FileEntryType == FileEntryType.File && y.FileEntryType == FileEntryType.File)
                                     cmp = c * ((File<T>)x).ContentLength.CompareTo(((File<T>)y).ContentLength);
                                 return cmp == 0 ? x.Title.EnumerableComparer(y.Title) : cmp;
                }
                ,
                SortedByType.AZ => (x, y) => c * x.Title.EnumerableComparer(y.Title),
                SortedByType.DateAndTime => (x, y) =>
                             {
                                 var cmp = c * DateTime.Compare(x.ModifiedOn, y.ModifiedOn);
                                 return cmp == 0 ? x.Title.EnumerableComparer(y.Title) : cmp;
                }
                ,
                SortedByType.DateAndTimeCreation => (x, y) =>
                    {
                        var cmp = c * DateTime.Compare(x.CreateOn, y.CreateOn);
                        return cmp == 0 ? x.Title.EnumerableComparer(y.Title) : cmp;
                }
                ,
                SortedByType.New => (x, y) =>
                        {
                            var isNewSortResult = x.IsNew.CompareTo(y.IsNew);
                            return c * (isNewSortResult == 0 ? DateTime.Compare(x.ModifiedOn, y.ModifiedOn) : isNewSortResult);
                }
                ,
                _ => (x, y) => c * x.Title.EnumerableComparer(y.Title),
                        };
            if (orderBy.SortedBy != SortedByType.New)
            {
                // folders on top
                var folders = entries.Where(r => r.FileEntryType == FileEntryType.Folder).ToList();
                var files = entries.Where(r => r.FileEntryType == FileEntryType.File).ToList();
                folders.Sort(sorter);
                files.Sort(sorter);

                return folders.Concat(files);
            }

            var result = entries.ToList();

            result.Sort(sorter);

            return result;
        }

        public Folder<string> GetFakeThirdpartyFolder<T>(IProviderInfo providerInfo, string parentFolderId = null)
        {
            //Fake folder. Don't send request to third party
            var folder = ServiceProvider.GetService<Folder<string>>();

            folder.FolderID = parentFolderId;

            folder.ID = providerInfo.RootFolderId;
            folder.CreateBy = providerInfo.Owner;
            folder.CreateOn = providerInfo.CreateOn;
            folder.FolderType = FolderType.DEFAULT;
            folder.ModifiedBy = providerInfo.Owner;
            folder.ModifiedOn = providerInfo.CreateOn;
            folder.ProviderId = providerInfo.ID;
            folder.ProviderKey = providerInfo.ProviderKey;
            folder.RootFolderCreator = providerInfo.Owner;
            folder.RootFolderId = providerInfo.RootFolderId;
            folder.RootFolderType = providerInfo.RootFolderType;
            folder.Shareable = false;
            folder.Title = providerInfo.CustomerTitle;
            folder.TotalFiles = 0;
            folder.TotalSubFolders = 0;

            return folder;
        }


        public async Task<List<FileEntry>> GetBreadCrumbs<T>(T folderId)
        {
            return await BreadCrumbsManager.GetBreadCrumbs(folderId);
        }

        public async Task<List<FileEntry>> GetBreadCrumbs<T>(T folderId, IFolderDao<T> folderDao)
        {
            return await BreadCrumbsManager.GetBreadCrumbs(folderId, folderDao);
        }


        public void SetFileStatus<T>(File<T> file)
        {
            if (file == null || file.ID == null) return;

            SetFileStatus(new List<File<T>>(1) { file });
        }

        public void SetFileStatus<T>(IEnumerable<File<T>> files)
        {
            var tagDao = DaoFactory.GetTagDao<T>();

            var tagsFavorite = tagDao.GetTags(AuthContext.CurrentAccount.ID, TagType.Favorite, files);
            var tagsTemplate = tagDao.GetTags(AuthContext.CurrentAccount.ID, TagType.Template, files);
            var tagsNew = tagDao.GetNewTags(AuthContext.CurrentAccount.ID, files);
            var tagsLocked = tagDao.GetTags(TagType.Locked, files.ToArray());

            foreach (var file in files)
            {
                if (tagsFavorite.Any(r => r.EntryId.Equals(file.ID)))
                {
                    file.IsFavorite = true;
                }

                if (tagsTemplate.Any(r => r.EntryId.Equals(file.ID)))
                {
                    file.IsTemplate = true;
                }

                if (tagsNew.Any(r => r.EntryId.Equals(file.ID)))
                {
                    file.IsNew = true;
                }

                var tagLocked = tagsLocked.FirstOrDefault(t => t.EntryId.Equals(file.ID));

                var lockedBy = tagLocked != null ? tagLocked.Owner : Guid.Empty;
                file.Locked = lockedBy != Guid.Empty;
                file.LockedBy = lockedBy != Guid.Empty && lockedBy != AuthContext.CurrentAccount.ID
                    ? Global.GetUserName(lockedBy)
                    : null;
            }
        }

        public bool FileLockedForMe<T>(T fileId, Guid userId = default)
        {
            return LockerManager.FileLockedForMe(fileId, userId);
        }

        public Guid FileLockedBy<T>(T fileId, ITagDao<T> tagDao)
        {
            return LockerManager.FileLockedBy(fileId, tagDao);
        }


        public async Task<File<T>> SaveEditing<T>(T fileId, string fileExtension, string downloadUri, Stream stream, string doc, string comment = null, bool checkRight = true, bool encrypted = false, ForcesaveType? forcesave = null)
        {
            var newExtension = string.IsNullOrEmpty(fileExtension)
                              ? FileUtility.GetFileExtension(downloadUri)
                              : fileExtension;

            var app = ThirdPartySelector.GetAppByFileId(fileId.ToString());
            if (app != null)
            {
                app.SaveFile(fileId.ToString(), newExtension, downloadUri, stream);
                return null;
            }

            var fileDao = DaoFactory.GetFileDao<T>();
            var (editLink, file) = await FileShareLink.Check(doc, false, fileDao);
            if (file == null)
            {
                file = await fileDao.GetFile(fileId);
            }

            if (file == null) throw new FileNotFoundException(FilesCommonResource.ErrorMassage_FileNotFound);
            var fileSecurity = FileSecurity;
            if (checkRight && !editLink && (!await fileSecurity.CanEdit(file) || UserManager.GetUsers(AuthContext.CurrentAccount.ID).IsVisitor(UserManager))) throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_EditFile);
            if (checkRight && FileLockedForMe(file.ID)) throw new Exception(FilesCommonResource.ErrorMassage_LockedFile);
            if (checkRight && (!forcesave.HasValue || forcesave.Value == ForcesaveType.None) && FileTracker.IsEditing(file.ID)) throw new Exception(FilesCommonResource.ErrorMassage_SecurityException_UpdateEditingFile);
            if (file.RootFolderType == FolderType.TRASH) throw new Exception(FilesCommonResource.ErrorMassage_ViewTrashItem);

            var currentExt = file.ConvertedExtension;
            if (string.IsNullOrEmpty(newExtension)) newExtension = FileUtility.GetInternalExtension(file.Title);

            var replaceVersion = false;
            if (file.Forcesave != ForcesaveType.None)
            {
                if (file.Forcesave == ForcesaveType.User && FilesSettingsHelper.StoreForcesave || encrypted)
                {
                    file.Version++;
                }
                else
                {
                    replaceVersion = true;
                }
            }
            else
            {
                if (file.Version != 1)
                {
                    file.VersionGroup++;
                }
                else
                {
                    var storeTemplate = GlobalStore.GetStoreTemplate();

                    var path = FileConstant.NewDocPath + Thread.CurrentThread.CurrentCulture + "/";
                    if (!storeTemplate.IsDirectory(path))
                    {
                        path = FileConstant.NewDocPath + "default/";
                    }
                    path += "new" + FileUtility.GetInternalExtension(file.Title);

                    //todo: think about the criteria for saving after creation
                    if (file.ContentLength != storeTemplate.GetFileSize("", path))
                    {
                        file.VersionGroup++;
                    }
                }
                file.Version++;
            }
            file.Forcesave = forcesave ?? ForcesaveType.None;

            if (string.IsNullOrEmpty(comment))
                comment = FilesCommonResource.CommentEdit;

            file.Encrypted = encrypted;

            file.ConvertedType = FileUtility.GetFileExtension(file.Title) != newExtension ? newExtension : null;

            if (file.ProviderEntry && !newExtension.Equals(currentExt))
            {
                if (FileUtility.ExtsConvertible.Keys.Contains(newExtension)
                    && FileUtility.ExtsConvertible[newExtension].Contains(currentExt))
                {
                    if (stream != null)
                    {
                        downloadUri = PathProvider.GetTempUrl(stream, newExtension);
                        downloadUri = DocumentServiceConnector.ReplaceCommunityAdress(downloadUri);
                    }

                    var key = DocumentServiceConnector.GenerateRevisionId(downloadUri);
                    DocumentServiceConnector.GetConvertedUri(downloadUri, newExtension, currentExt, key, null, false, out downloadUri);

                    stream = null;
                }
                else
                {
                    file.ID = default;
                    file.Title = FileUtility.ReplaceFileExtension(file.Title, newExtension);
                }

                file.ConvertedType = null;
            }

            using (var tmpStream = new MemoryStream())
            {
                if (stream != null)
                {
                    stream.CopyTo(tmpStream);
                }
                else
                {
                    // hack. http://ubuntuforums.org/showthread.php?t=1841740
                    if (WorkContext.IsMono)
                    {
                        ServicePointManager.ServerCertificateValidationCallback += (s, ce, ca, p) => true;
                    }

                    var req = (HttpWebRequest)WebRequest.Create(downloadUri);
                    using var editedFileStream = new ResponseStream(req.GetResponse());
                        editedFileStream.CopyTo(tmpStream);
                    }
                tmpStream.Position = 0;

                file.ContentLength = tmpStream.Length;
                file.Comment = string.IsNullOrEmpty(comment) ? null : comment;
                if (replaceVersion)
                {
                    file = await fileDao.ReplaceFileVersion(file, tmpStream);
                }
                else
                {
                    file = await fileDao.SaveFile(file, tmpStream);
                }
            }

            await FileMarker.MarkAsNew(file);
            await FileMarker.RemoveMarkAsNew(file);
            return file;
        }

        public async Task TrackEditing<T>(T fileId, Guid tabId, Guid userId, string doc, bool editingAlone = false)
        {
            bool checkRight;
            if (FileTracker.GetEditingBy(fileId).Contains(userId))
            {
                checkRight = FileTracker.ProlongEditing(fileId, tabId, userId, editingAlone);
                if (!checkRight) return;
            }

            var fileDao = DaoFactory.GetFileDao<T>();
            var (editLink, file) = await FileShareLink.Check(doc, false, fileDao);
            if (file == null)
                file = await fileDao.GetFile(fileId);

            if (file == null) throw new FileNotFoundException(FilesCommonResource.ErrorMassage_FileNotFound);
            var fileSecurity = FileSecurity;
            if (!editLink
                && (!await fileSecurity.CanEdit(file, userId)
                    && !await fileSecurity.CanCustomFilterEdit(file, userId)
                    && !await fileSecurity.CanReview(file, userId)
                    && !await fileSecurity.CanFillForms(file, userId)
                    && !await fileSecurity.CanComment(file, userId)
                    || UserManager.GetUsers(userId).IsVisitor(UserManager)))
            {
                throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_EditFile);
            }
            if (FileLockedForMe(file.ID, userId)) throw new Exception(FilesCommonResource.ErrorMassage_LockedFile);
            if (file.RootFolderType == FolderType.TRASH) throw new Exception(FilesCommonResource.ErrorMassage_ViewTrashItem);

            checkRight = FileTracker.ProlongEditing(fileId, tabId, userId, editingAlone);
            if (checkRight)
            {
                FileTracker.ChangeRight(fileId, userId, false);
            }
        }

        public async Task<File<T>> UpdateToVersionFile<T>(T fileId, int version, string doc = null, bool checkRight = true)
        {
            var fileDao = DaoFactory.GetFileDao<T>();
            if (version < 1) throw new ArgumentNullException("version");

            var (editLink, fromFile) = await FileShareLink.Check(doc, false, fileDao);

            if (fromFile == null)
                fromFile = await fileDao.GetFile(fileId);

            if (fromFile == null) throw new FileNotFoundException(FilesCommonResource.ErrorMassage_FileNotFound);

            if (fromFile.Version != version)
                fromFile = await fileDao.GetFile(fromFile.ID, Math.Min(fromFile.Version, version));

            if (fromFile == null) throw new FileNotFoundException(FilesCommonResource.ErrorMassage_FileNotFound);
            if (checkRight && !editLink && (!await FileSecurity.CanEdit(fromFile) || UserManager.GetUsers(AuthContext.CurrentAccount.ID).IsVisitor(UserManager))) throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_EditFile);
            if (FileLockedForMe(fromFile.ID)) throw new Exception(FilesCommonResource.ErrorMassage_LockedFile);
            if (checkRight && FileTracker.IsEditing(fromFile.ID)) throw new Exception(FilesCommonResource.ErrorMassage_SecurityException_UpdateEditingFile);
            if (fromFile.RootFolderType == FolderType.TRASH) throw new Exception(FilesCommonResource.ErrorMassage_ViewTrashItem);
            if (fromFile.ProviderEntry) throw new Exception(FilesCommonResource.ErrorMassage_BadRequest);
            if (fromFile.Encrypted) throw new Exception(FilesCommonResource.ErrorMassage_NotSupportedFormat);

            var exists = cache.Get<string>(UPDATE_LIST + fileId.ToString()) != null;
            if (exists)
            {
                throw new Exception(FilesCommonResource.ErrorMassage_UpdateEditingFile);
            }
            else
            {
                cache.Insert(UPDATE_LIST + fileId.ToString(), fileId.ToString(), TimeSpan.FromMinutes(2));
            }

            try
            {
                var currFile = await fileDao.GetFile(fileId);
                var newFile = ServiceProvider.GetService<File<T>>();

                newFile.ID = fromFile.ID;
                newFile.Version = currFile.Version + 1;
                newFile.VersionGroup = currFile.VersionGroup;
                newFile.Title = FileUtility.ReplaceFileExtension(currFile.Title, FileUtility.GetFileExtension(fromFile.Title));
                newFile.FileStatus = currFile.FileStatus;
                newFile.FolderID = currFile.FolderID;
                newFile.CreateBy = currFile.CreateBy;
                newFile.CreateOn = currFile.CreateOn;
                newFile.ModifiedBy = fromFile.ModifiedBy;
                newFile.ModifiedOn = fromFile.ModifiedOn;
                newFile.ConvertedType = fromFile.ConvertedType;
                newFile.Comment = string.Format(FilesCommonResource.CommentRevert, fromFile.ModifiedOnString);
                newFile.Encrypted = fromFile.Encrypted;

                using (var stream = fileDao.GetFileStream(fromFile))
                {
                    newFile.ContentLength = stream.CanSeek ? stream.Length : fromFile.ContentLength;
                    newFile = await fileDao.SaveFile(newFile, stream);
                }

                await FileMarker.MarkAsNew(newFile);

                SetFileStatus(newFile);

                newFile.Access = fromFile.Access;

                if (newFile.IsTemplate
                    && !FileUtility.ExtsWebTemplate.Contains(FileUtility.GetFileExtension(newFile.Title), StringComparer.CurrentCultureIgnoreCase))
                {
                    var tagTemplate = Tag.Template(AuthContext.CurrentAccount.ID, newFile);
                    var tagDao = DaoFactory.GetTagDao<T>();
                    tagDao.RemoveTags(tagTemplate);

                    newFile.IsTemplate = false;
                }

                return newFile;
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("Error on update {0} to version {1}", fileId, version), e);
                throw new Exception(e.Message, e);
            }
            finally
            {
                cache.Remove(UPDATE_LIST + fromFile.ID);
            }
        }

        public async Task<File<T>> CompleteVersionFile<T>(T fileId, int version, bool continueVersion, bool checkRight = true)
        {
            var fileDao = DaoFactory.GetFileDao<T>();
            var fileVersion = version > 0
? await fileDao.GetFile(fileId, version)
: await fileDao.GetFile(fileId);
            if (fileVersion == null) throw new FileNotFoundException(FilesCommonResource.ErrorMassage_FileNotFound);
            if (checkRight && (!await FileSecurity.CanEdit(fileVersion) || UserManager.GetUsers(AuthContext.CurrentAccount.ID).IsVisitor(UserManager))) throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_EditFile);
            if (FileLockedForMe(fileVersion.ID)) throw new Exception(FilesCommonResource.ErrorMassage_LockedFile);
            if (fileVersion.RootFolderType == FolderType.TRASH) throw new Exception(FilesCommonResource.ErrorMassage_ViewTrashItem);
            if (fileVersion.ProviderEntry) throw new Exception(FilesCommonResource.ErrorMassage_BadRequest);

            var lastVersionFile = await fileDao.GetFile(fileVersion.ID);

            if (continueVersion)
            {
                if (lastVersionFile.VersionGroup > 1)
                {
                    await fileDao.ContinueVersion(fileVersion.ID, fileVersion.Version);
                    lastVersionFile.VersionGroup--;
                }
            }
            else
            {
                if (!FileTracker.IsEditing(lastVersionFile.ID))
                {
                    if (fileVersion.Version == lastVersionFile.Version)
                    {
                        lastVersionFile = await UpdateToVersionFile(fileVersion.ID, fileVersion.Version, null, checkRight);
                    }

                    await fileDao.CompleteVersion(fileVersion.ID, fileVersion.Version);
                    lastVersionFile.VersionGroup++;
                }
            }

            SetFileStatus(lastVersionFile);

            return lastVersionFile;
        }

        public async Task<(bool, File<T>)> FileRename<T>(T fileId, string title)
        {
            var fileDao = DaoFactory.GetFileDao<T>();
            var file = await fileDao.GetFile(fileId);
            if (file == null) throw new FileNotFoundException(FilesCommonResource.ErrorMassage_FileNotFound);
            if (!await FileSecurity.CanEdit(file)) throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_RenameFile);
            if (!await FileSecurity.CanDelete(file) && UserManager.GetUsers(AuthContext.CurrentAccount.ID).IsVisitor(UserManager)) throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_RenameFile);
            if (FileLockedForMe(file.ID)) throw new Exception(FilesCommonResource.ErrorMassage_LockedFile);
            if (file.ProviderEntry && FileTracker.IsEditing(file.ID)) throw new Exception(FilesCommonResource.ErrorMassage_UpdateEditingFile);
            if (file.RootFolderType == FolderType.TRASH) throw new Exception(FilesCommonResource.ErrorMassage_ViewTrashItem);

            title = Global.ReplaceInvalidCharsAndTruncate(title);

            var ext = FileUtility.GetFileExtension(file.Title);
            if (string.Compare(ext, FileUtility.GetFileExtension(title), true) != 0)
            {
                title += ext;
            }

            var fileAccess = file.Access;

            var renamed = false;
            if (string.Compare(file.Title, title, false) != 0)
            {
                var newFileID = await fileDao.FileRename(file, title);

                file = await fileDao.GetFile(newFileID);
                file.Access = fileAccess;

                await DocumentServiceHelper.RenameFile(file, fileDao);

                renamed = true;
            }

            SetFileStatus(file);

            return (renamed, file);
        }

        public void MarkAsRecent<T>(File<T> file)
        {
            if (file.Encrypted || file.ProviderEntry) throw new NotSupportedException();

            var tagDao = DaoFactory.GetTagDao<T>();
            var userID = AuthContext.CurrentAccount.ID;

            var tag = Tag.Recent(userID, file);
            tagDao.SaveTags(tag);
        }


        //Long operation
        public async Task DeleteSubitems<T>(T parentId, IFolderDao<T> folderDao, IFileDao<T> fileDao)
        {
            var folders = await folderDao.GetFolders(parentId);
            foreach (var folder in folders)
            {
                await DeleteSubitems(folder.ID, folderDao, fileDao);

                Logger.InfoFormat("Delete folder {0} in {1}", folder.ID, parentId);
                await folderDao.DeleteFolder(folder.ID);
            }

            var files = await fileDao.GetFiles(parentId, null, FilterType.None, false, Guid.Empty, string.Empty, true);
            foreach (var file in files)
            {
                Logger.InfoFormat("Delete file {0} in {1}", file.ID, parentId);
                await fileDao.DeleteFile(file.ID);
            }
        }

        public async Task MoveSharedItems<T>(T parentId, T toId, IFolderDao<T> folderDao, IFileDao<T> fileDao)
        {
            var fileSecurity = FileSecurity;

            var folders = await folderDao.GetFolders(parentId);
            foreach (var folder in folders)
            {
                var shared = folder.Shared
                             && fileSecurity.GetShares(folder).Any(record => record.Share != FileShare.Restrict);
                if (shared)
                {
                    Logger.InfoFormat("Move shared folder {0} from {1} to {2}", folder.ID, parentId, toId);
                    await folderDao.MoveFolder(folder.ID, toId, null);
                }
                else
                {
                    await MoveSharedItems(folder.ID, toId, folderDao, fileDao);
                }
            }

            var files = (await fileDao.GetFiles(parentId, null, FilterType.None, false, Guid.Empty, string.Empty, true))
                .Where(file => file.Shared &&
                fileSecurity.GetShares(file).Any(record => record.Subject != FileConstant.ShareLinkId && record.Share != FileShare.Restrict));

            foreach (var file in files)
            {
                Logger.InfoFormat("Move shared file {0} from {1} to {2}", file.ID, parentId, toId);
                await fileDao.MoveFile(file.ID, toId);
            }
        }

        public static async Task ReassignItems<T>(T parentId, Guid fromUserId, Guid toUserId, IFolderDao<T> folderDao, IFileDao<T> fileDao)
        {
            var fileIds = (await fileDao.GetFiles(parentId, new OrderBy(SortedByType.AZ, true), FilterType.ByUser, false, fromUserId, null, true, true))
                                 .Where(file => file.CreateBy == fromUserId).Select(file => file.ID);

            fileDao.ReassignFiles(fileIds.ToArray(), toUserId);

            var folderIds = (await folderDao.GetFolders(parentId, new OrderBy(SortedByType.AZ, true), FilterType.ByUser, false, fromUserId, null, true))
                                     .Where(folder => folder.CreateBy == fromUserId).Select(folder => folder.ID);

            await folderDao.ReassignFolders(folderIds.ToArray(), toUserId);
        }
    }
}