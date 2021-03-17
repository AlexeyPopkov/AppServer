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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ASC.Common;
using ASC.Core.Tenants;
using ASC.Files.Core;
using ASC.Files.Core.Resources;
using ASC.MessagingSystem;
using ASC.Web.Files.Helpers;
using ASC.Web.Files.Utils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace ASC.Web.Files.Services.WCFService.FileOperations
{
    internal class FileDeleteOperationData<T> : FileOperationData<T>
    {
        public bool IgnoreException { get; }
        public bool Immediately { get; }
        public IDictionary<string, StringValues> Headers { get; }

        public FileDeleteOperationData(IEnumerable<object> folders, IEnumerable<object> files, Tenant tenant,
            bool holdResult = true, bool ignoreException = false, bool immediately = false, IDictionary<string, StringValues> headers = null)
            : this(folders.OfType<T>(), files.OfType<T>(), tenant, holdResult, ignoreException, immediately, headers)
        {
        }

        public FileDeleteOperationData(IEnumerable<T> folders, IEnumerable<T> files, Tenant tenant,
            bool holdResult = true, bool ignoreException = false, bool immediately = false, IDictionary<string, StringValues> headers = null)
            : base(folders, files, tenant, holdResult)
        {
            IgnoreException = ignoreException;
            Immediately = immediately;
            Headers = headers;
        }
    }

    class FileDeleteOperation : ComposeFileOperation<FileDeleteOperationData<string>, FileDeleteOperationData<int>>
    {
        public FileDeleteOperation(Guid userId, FileOperation<FileDeleteOperationData<string>, string> f1, FileOperation<FileDeleteOperationData<int>, int> f2)
            : base(userId, f1, f2)
        {
        }

        public override FileOperationType OperationType
        {
            get { return FileOperationType.Delete; }
        }
    }

    class FileDeleteOperation<T> : FileOperation<FileDeleteOperationData<T>, T>
    {
        private int _trashId;
        private readonly bool _ignoreException;
        private readonly bool _immediately;
        private readonly IDictionary<string, StringValues> _headers;

        public override FileOperationType OperationType
        {
            get { return FileOperationType.Delete; }
        }


        public FileDeleteOperation(IServiceProvider serviceProvider, Guid userId, FileDeleteOperationData<T> fileOperationData)
            : base(serviceProvider, userId, fileOperationData)
        {
            _ignoreException = fileOperationData.IgnoreException;
            _immediately = fileOperationData.Immediately;
            _headers = fileOperationData.Headers;
        }


        protected override async Task Do(IServiceScope scope)
        {
            var folderDaoInt = scope.ServiceProvider.GetService<IFolderDao<int>>();
            var folderDao = scope.ServiceProvider.GetService<IFolderDao<T>>();
            _trashId = await folderDaoInt.GetFolderIDTrash(true);

            Folder<T> root = null;
            if (0 < Folders.Count)
            {
                root = await folderDao.GetRootFolder(Folders[0]);
            }
            else if (0 < Files.Count)
            {
                root = await folderDao.GetRootFolderByFile(Files[0]);
            }
            if (root != null)
            {
                Status += string.Format("folder_{0}{1}", root.ID, SPLIT_CHAR);
            }
            await DeleteFiles(Files, scope);
            await DeleteFolders(Folders, scope);
        }

        private async Task DeleteFolders(IEnumerable<T> folderIds, IServiceScope scope)
        {
            var ProviderDao = scope.ServiceProvider.GetService<IProviderDao>();
            var fileDao = scope.ServiceProvider.GetService<IFileDao<T>>();
            var folderDao = scope.ServiceProvider.GetService<IFolderDao<T>>();
            var scopeClass = scope.ServiceProvider.GetService<FileDeleteOperationScope>();
            var (fileMarker, filesMessageService) = scopeClass;
            foreach (var folderId in folderIds)
            {
                CancellationToken.ThrowIfCancellationRequested();

                var folder = await folderDao.GetFolder(folderId);
                T canCalculate = default;
                if (folder == null)
                {
                    Error = FilesCommonResource.ErrorMassage_FolderNotFound;
                }
                else if (folder.FolderType != FolderType.DEFAULT && folder.FolderType != FolderType.BUNCH)
                {
                    Error = FilesCommonResource.ErrorMassage_SecurityException_DeleteFolder;
                }
                else if (!_ignoreException && !await FilesSecurity.CanDelete(folder))
                {
                    canCalculate = folderDao.CanCalculateSubitems(folderId) ? default : folderId;

                    Error = FilesCommonResource.ErrorMassage_SecurityException_DeleteFolder;
                }
                else
                {
                    canCalculate = folderDao.CanCalculateSubitems(folderId) ? default : folderId;

                    await fileMarker.RemoveMarkAsNewForAll(folder);
                    if (folder.ProviderEntry && folder.ID.Equals(folder.RootFolderId))
                    {
                        if (ProviderDao != null)
                        {
                            ProviderDao.RemoveProviderInfo(folder.ProviderId);
                            filesMessageService.Send(folder, _headers, MessageAction.ThirdPartyDeleted, folder.ID.ToString(), folder.ProviderKey);
                        }

                        ProcessedFolder(folderId);
                    }
                    else
                    {
                        var immediately = _immediately || !folderDao.UseTrashForRemove(folder);
                        if (immediately && folderDao.UseRecursiveOperation(folder.ID, default(T)))
                        {
                            await DeleteFiles(await fileDao.GetFiles(folder.ID), scope);
                            await DeleteFolders((await folderDao.GetFolders(folder.ID)).Select(f => f.ID).ToList(), scope);

                            if (await folderDao.IsEmpty(folder.ID))
                            {
                                await folderDao.DeleteFolder(folder.ID);
                                filesMessageService.Send(folder, _headers, MessageAction.FolderDeleted, folder.Title);

                                ProcessedFolder(folderId);
                            }
                        }
                        else
                        {
                            var files = fileDao.GetFiles(folder.ID, new OrderBy(SortedByType.AZ, true), FilterType.FilesOnly, false, Guid.Empty, string.Empty, false, true);
                            var tmpError = _ignoreException ? null : await WithError(scope, files, true);
                            if (!string.IsNullOrEmpty(tmpError))
                            {
                                Error = tmpError;
                            }
                            else
                            {
                                if (immediately)
                                {
                                    await folderDao.DeleteFolder(folder.ID);
                                    filesMessageService.Send(folder, _headers, MessageAction.FolderDeleted, folder.Title);
                                }
                                else
                                {
                                    await folderDao.MoveFolder(folder.ID, _trashId, CancellationToken);
                                    filesMessageService.Send(folder, _headers, MessageAction.FolderMovedToTrash, folder.Title);
                                }

                                ProcessedFolder(folderId);
                            }
                        }
                    }
                }
                ProgressStep(canCalculate);
            }
        }

        private async Task DeleteFiles(IEnumerable<T> fileIds, IServiceScope scope)
        {
            var fileDao = scope.ServiceProvider.GetService<IFileDao<T>>();
            var folderDao = scope.ServiceProvider.GetService<IFolderDao<T>>();
            var scopeClass = scope.ServiceProvider.GetService<FileDeleteOperationScope>();
            var (fileMarker, filesMessageService) = scopeClass;
            foreach (var fileId in fileIds)
            {
                CancellationToken.ThrowIfCancellationRequested();

                var file = await fileDao.GetFile(fileId);
                if (file == null)
                {
                    Error = FilesCommonResource.ErrorMassage_FileNotFound;
                }
                else
                {
                    var tmpError = _ignoreException ? null : await WithError(scope, new[] { file }.ToAsyncEnumerable(), false);
                    if (!string.IsNullOrEmpty(tmpError))
                    {
                        Error = tmpError;
                    }
                    else
                    {
                        await fileMarker.RemoveMarkAsNewForAll(file);
                        if (!_immediately && fileDao.UseTrashForRemove(file))
                        {
                            await fileDao.MoveFile(file.ID, _trashId);
                            filesMessageService.Send(file, _headers, MessageAction.FileMovedToTrash, file.Title);
                        }
                        else
                        {
                            try
                            {
                                await fileDao.DeleteFile(file.ID);
                                filesMessageService.Send(file, _headers, MessageAction.FileDeleted, file.Title);
                            }
                            catch (Exception ex)
                            {
                                Error = ex.Message;
                                Logger.Error(Error, ex);
                            }
                        }
                        ProcessedFile(fileId);
                    }
                }
                ProgressStep(fileId: folderDao.CanCalculateSubitems(fileId) ? default : fileId);
            }
        }

        private async Task<string> WithError(IServiceScope scope, IAsyncEnumerable<File<T>> files, bool folder)
        {
            var entryManager = scope.ServiceProvider.GetService<EntryManager>();
            var fileTracker = scope.ServiceProvider.GetService<FileTrackerHelper>();

            await foreach (var file in files)
            {
                if (!await FilesSecurity.CanDelete(file))
                {
                    return FilesCommonResource.ErrorMassage_SecurityException_DeleteFile;
                }
                if (entryManager.FileLockedForMe(file.ID))
                {
                    return FilesCommonResource.ErrorMassage_LockedFile;
                }
                if (fileTracker.IsEditing(file.ID))
                {
                    return folder ? FilesCommonResource.ErrorMassage_SecurityException_DeleteEditingFolder : FilesCommonResource.ErrorMassage_SecurityException_DeleteEditingFile;
                }
            }
            return null;
        }
    }

    [Scope]
    public class FileDeleteOperationScope
    {
        private FileMarker FileMarker { get; }
        private FilesMessageService FilesMessageService { get; }

        public FileDeleteOperationScope(FileMarker fileMarker, FilesMessageService filesMessageService)
        {
            FileMarker = fileMarker;
            FilesMessageService = filesMessageService;
        }

        public void Deconstruct(out FileMarker fileMarker, out FilesMessageService filesMessageService)
        {
            fileMarker = FileMarker;
            filesMessageService = FilesMessageService;
        }
    }
}