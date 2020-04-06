/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Data;
using System.IO;
using System.Linq;
using ASC.Common;
using ASC.Common.Logging;
using ASC.Common.Web;
using ASC.Data.Storage;
using ASC.Files.Core.Security;
using ASC.Mail.Core.Dao.Expressions.Attachment;
using ASC.Mail.Core.Dao.Expressions.Message;
using ASC.Mail.Core.Entities;
using ASC.Mail.Data.Storage;
using ASC.Mail.Enums;
using ASC.Mail.Exceptions;
using ASC.Mail.Extensions;
using ASC.Mail.Models;
using ASC.Web.Core.Files;
using ASC.Web.Files.Api;
using ASC.Web.Files.Utils;
using Microsoft.Extensions.Options;

namespace ASC.Mail.Core.Engine
{
    public class AttachmentEngine
    {
        public QuotaEngine QuotaEngine { get; }
        public ChainEngine ChainEngine { get; }
        public IndexEngine IndexEngine { get; }
        public MessageEngine MessageEngine { get; }
        public DaoFactory DaoFactory { get; }
        public StorageFactory StorageFactory { get; }
        public StorageManager StorageManager { get; }
        public FilesIntegration FilesIntegration { get; }
        public FileSecurity FilesSeurity { get; }
        public FileConverter FileConverter { get; }
        public ILog Log { get; }

        public AttachmentEngine(
            QuotaEngine quotaEngine,
            ChainEngine chainEngine,
            IndexEngine indexEngine,
            MessageEngine messageEngine,
            DaoFactory daoFactory,
            StorageFactory storageFactory,
            StorageManager storageManager,
            FilesIntegration filesIntegration,
            FileSecurity filesSeurity,
            FileConverter fileConverter,
            IOptionsMonitor<ILog> option)
        {
            QuotaEngine = quotaEngine;
            ChainEngine = chainEngine;
            IndexEngine = indexEngine;
            MessageEngine = messageEngine;
            DaoFactory = daoFactory;
            StorageFactory = storageFactory;
            StorageManager = storageManager;
            FilesIntegration = filesIntegration;
            FilesSeurity = filesSeurity;
            FileConverter = fileConverter;
            Log = option.Get("ASC.Mail.AttachmentEngine");
        }

        public MailAttachmentData GetAttachment(IAttachmentExp exp)
        {
            var attachment = DaoFactory.AttachmentDao.GetAttachment(exp);

            return ToAttachmentData(attachment);
        }

        public List<MailAttachmentData> GetAttachments(IAttachmentsExp exp)
        {
            var attachments = DaoFactory.AttachmentDao.GetAttachments(exp);

            return attachments.ConvertAll(ToAttachmentData);
        }

        public long GetAttachmentsSize(IAttachmentsExp exp)
        {
            var size = DaoFactory.AttachmentDao.GetAttachmentsSize(exp);

            return size;
        }

        public int GetAttachmentNextFileNumber(IAttachmentsExp exp)
        {
            var number = DaoFactory.AttachmentDao.GetAttachmentsMaxFileNumber(exp);

            number++;

            return number;
        }

        public MailAttachmentData AttachFileFromDocuments(int tenant, string user, int messageId, string fileId, string version, bool needSaveToTemp = false)
        {
            MailAttachmentData result;

            var fileDao = FilesIntegration.GetFileDao();

            var file = string.IsNullOrEmpty(version)
                           ? fileDao.GetFile(fileId)
                           : fileDao.GetFile(fileId, Convert.ToInt32(version));

            if (file == null)
                throw new AttachmentsException(AttachmentsException.Types.DocumentNotFound, "File not found.");

            if (!FilesSeurity.CanRead(file))
                throw new AttachmentsException(AttachmentsException.Types.DocumentAccessDenied,
                                               "Access denied.");

            if (!fileDao.IsExistOnStorage(file))
            {
                throw new AttachmentsException(AttachmentsException.Types.DocumentNotFound,
                                               "File not exists on storage.");
            }

            Log.InfoFormat("Original file id: {0}", file.ID);
            Log.InfoFormat("Original file name: {0}", file.Title);
            var fileExt = FileUtility.GetFileExtension(file.Title);
            var curFileType = FileUtility.GetFileTypeByFileName(file.Title);
            Log.InfoFormat("File converted type: {0}", file.ConvertedType);

            if (file.ConvertedType != null)
            {
                switch (curFileType)
                {
                    case FileType.Image:
                        fileExt = file.ConvertedType == ".zip" ? ".pptt" : file.ConvertedType;
                        break;
                    case FileType.Spreadsheet:
                        fileExt = file.ConvertedType != ".xlsx" ? ".xlst" : file.ConvertedType;
                        break;
                    default:
                        if (file.ConvertedType == ".doct" || file.ConvertedType == ".xlst" || file.ConvertedType == ".pptt")
                            fileExt = file.ConvertedType;
                        break;
                }
            }

            var convertToExt = string.Empty;
            switch (curFileType)
            {
                case FileType.Document:
                    if (fileExt == ".doct")
                        convertToExt = ".docx";
                    break;
                case FileType.Spreadsheet:
                    if (fileExt == ".xlst")
                        convertToExt = ".xlsx";
                    break;
                case FileType.Presentation:
                    if (fileExt == ".pptt")
                        convertToExt = ".pptx";
                    break;
            }

            if (!string.IsNullOrEmpty(convertToExt) && fileExt != convertToExt)
            {
                var fileName = Path.ChangeExtension(file.Title, convertToExt);
                Log.InfoFormat("Changed file name - {0} for file {1}:", fileName, file.ID);

                using var readStream = FileConverter.Exec(file, convertToExt);

                if (readStream == null)
                    throw new AttachmentsException(AttachmentsException.Types.DocumentAccessDenied, "Access denied.");

                using var memStream = new MemoryStream();

                readStream.StreamCopyTo(memStream);
                result = AttachFileToDraft(tenant, user, messageId, fileName, memStream, memStream.Length, null, needSaveToTemp);
                Log.InfoFormat("Attached attachment: ID - {0}, Name - {1}, StoredUrl - {2}", result.fileName, result.fileName, result.storedFileUrl);
            }
            else
            {
                using var readStream = fileDao.GetFileStream(file);

                if (readStream == null)
                    throw new AttachmentsException(AttachmentsException.Types.DocumentAccessDenied, "Access denied.");

                result = AttachFileToDraft(tenant, user, messageId, file.Title, readStream, readStream.CanSeek ? readStream.Length : file.ContentLength, null, needSaveToTemp);
                Log.InfoFormat("Attached attachment: ID - {0}, Name - {1}, StoredUrl - {2}", result.fileName, result.fileName, result.storedFileUrl);
            }

            return result;
        }

        public MailAttachmentData AttachFile(int tenant, string user, MailMessageData message,
            string name, Stream inputStream, long contentLength, string contentType = null, bool needSaveToTemp = false)
        {
            if (message == null)
                throw new AttachmentsException(AttachmentsException.Types.MessageNotFound, "Message not found.");

            if (string.IsNullOrEmpty(message.StreamId))
                throw new AttachmentsException(AttachmentsException.Types.MessageNotFound, "StreamId is empty.");

            var messageId = message.Id;

            var totalSize = GetAttachmentsSize(new ConcreteMessageAttachmentsExp(messageId, tenant, user));

            totalSize += contentLength;

            if (totalSize > Defines.ATTACHMENTS_TOTAL_SIZE_LIMIT)
                throw new AttachmentsException(AttachmentsException.Types.TotalSizeExceeded,
                    "Total size of all files exceeds limit!");

            var fileNumber =
                GetAttachmentNextFileNumber(new ConcreteMessageAttachmentsExp(messageId, tenant,
                    user));

            var attachment = new MailAttachmentData
            {
                fileName = name,
                contentType = string.IsNullOrEmpty(contentType) ? MimeMapping.GetMimeMapping(name) : contentType,
                needSaveToTemp = needSaveToTemp,
                fileNumber = fileNumber,
                size = contentLength,
                data = inputStream.ReadToEnd(),
                streamId = message.StreamId,
                tenant = tenant,
                user = user,
                mailboxId = message.MailboxId
            };

            QuotaEngine.QuotaUsedAdd(contentLength);

            try
            {
                StorageManager.StoreAttachmentWithoutQuota(attachment);
            }
            catch
            {
                QuotaEngine.QuotaUsedDelete(contentLength);
                throw;
            }

            if (!needSaveToTemp)
            {
                int attachCount;

                using (var tx = DaoFactory.BeginTransaction())
                {
                    attachment.fileId = DaoFactory.AttachmentDao.SaveAttachment(attachment.ToAttachmnet(messageId));

                    attachCount = DaoFactory.AttachmentDao.GetAttachmentsCount(
                        new ConcreteMessageAttachmentsExp(messageId, tenant, user));

                    DaoFactory.MailInfoDao.SetFieldValue(
                        SimpleMessagesExp.CreateBuilder(tenant, user)
                            .SetMessageId(messageId)
                            .Build(),
                        "AttachCount",
                        attachCount);

                    ChainEngine.UpdateMessageChainAttachmentsFlag(tenant, user, messageId);

                    tx.Commit();
                }

                if (attachCount == 1)
                {
                    var data = new MailWrapper
                    {
                        HasAttachments = true
                    };

                    IndexEngine.Update(data, s => s.Where(m => m.Id, messageId), wrapper => wrapper.HasAttachments);
                }
            }

            return attachment;
        }

        public MailAttachmentData AttachFileToDraft(int tenant, string user, int messageId,
            string name, Stream inputStream, long contentLength, string contentType = null, bool needSaveToTemp = false)
        {
            if (messageId < 1)
                throw new AttachmentsException(AttachmentsException.Types.BadParams, "Field 'id_message' must have non-negative value.");

            if (tenant < 0)
                throw new AttachmentsException(AttachmentsException.Types.BadParams, "Field 'id_tenant' must have non-negative value.");

            if (String.IsNullOrEmpty(user))
                throw new AttachmentsException(AttachmentsException.Types.BadParams, "Field 'id_user' is empty.");

            if (contentLength == 0)
                throw new AttachmentsException(AttachmentsException.Types.EmptyFile, "Empty files not supported.");

            var message = MessageEngine.GetMessage(messageId, new MailMessageData.Options());

            if (message.Folder != FolderType.Draft && message.Folder != FolderType.Templates && message.Folder != FolderType.Sending)
                throw new AttachmentsException(AttachmentsException.Types.BadParams, "Message is not a draft or templates.");

            return AttachFile(tenant, user, message, name, inputStream, contentLength, contentType, needSaveToTemp);
        }

        public void StoreAttachmentCopy(int tenant, string user, MailAttachmentData attachment, string streamId)
        {
            try
            {
                if (attachment.streamId.Equals(streamId) && !attachment.isTemp) return;

                string s3Key;

                var dataClient = StorageFactory.GetMailStorage(tenant);

                if (attachment.needSaveToTemp || attachment.isTemp)
                {
                    s3Key = MailStoragePathCombiner.GetTempStoredFilePath(attachment);
                }
                else
                {
                    s3Key = MailStoragePathCombiner.GerStoredFilePath(attachment);
                }

                if (!dataClient.IsFile(s3Key)) return;

                attachment.fileNumber =
                    !string.IsNullOrEmpty(attachment.contentId) //Upload hack: embedded attachment have to be saved in 0 folder
                        ? 0
                        : attachment.fileNumber;

                var newS3Key = MailStoragePathCombiner.GetFileKey(user, streamId, attachment.fileNumber,
                                                                    attachment.storedName);

                var copyS3Url = dataClient.Copy(s3Key, string.Empty, newS3Key);

                attachment.storedFileUrl = MailStoragePathCombiner.GetStoredUrl(copyS3Url);

                attachment.streamId = streamId;

                attachment.tempStoredUrl = null;

                Log.DebugFormat("StoreAttachmentCopy() tenant='{0}', user_id='{1}', stream_id='{2}', new_s3_key='{3}', copy_s3_url='{4}', storedFileUrl='{5}',  filename='{6}'",
                    tenant, user, streamId, newS3Key, copyS3Url, attachment.storedFileUrl, attachment.fileName);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("CopyAttachment(). filename='{0}', ctype='{1}' Exception:\r\n{2}\r\n",
                           attachment.fileName,
                           attachment.contentType,
                    ex.ToString());

                throw;
            }
        }

        public void DeleteMessageAttachments(int tenant, string user, int messageId, List<int> attachmentIds)
        {
            long usedQuota;
            int attachCount;

            using (var tx = DaoFactory.BeginTransaction())
            {
                var exp = new ConcreteMessageAttachmentsExp(messageId, tenant, user, attachmentIds,
                    onlyEmbedded: null);

                usedQuota = DaoFactory.AttachmentDao.GetAttachmentsSize(exp);

                DaoFactory.AttachmentDao.SetAttachmnetsRemoved(exp);

                attachCount = DaoFactory.AttachmentDao.GetAttachmentsCount(
                    new ConcreteMessageAttachmentsExp(messageId, tenant, user));

                DaoFactory.MailInfoDao.SetFieldValue(
                    SimpleMessagesExp.CreateBuilder(tenant, user)
                        .SetMessageId(messageId)
                        .Build(),
                    "AttachCount",
                    attachCount);

                ChainEngine.UpdateMessageChainAttachmentsFlag(tenant, user, messageId);

                tx.Commit();
            }

            if (attachCount == 0)
            {
                var data = new MailWrapper
                {
                    HasAttachments = false
                };

                IndexEngine.Update(data, s => s.Where(m => m.Id, messageId), wrapper => wrapper.HasAttachments);
            }

            if (usedQuota <= 0)
                return;

            QuotaEngine.QuotaUsedDelete(usedQuota);
        }

        public void StoreAttachments(MailBoxData mailBoxData, List<MailAttachmentData> attachments, string streamId)
        {
            if (!attachments.Any() || string.IsNullOrEmpty(streamId)) return;

            try
            {
                var quotaAddSize = attachments.Sum(a => a.data != null ? a.data.LongLength : a.dataStream.Length);

                foreach (var attachment in attachments)
                {
                    var isAttachmentNameHasBadName = string.IsNullOrEmpty(attachment.fileName)
                                                         || attachment.fileName.IndexOfAny(Path.GetInvalidPathChars()) != -1
                                                         || attachment.fileName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1;
                    if (isAttachmentNameHasBadName)
                    {
                        attachment.fileName = string.Format("attacment{0}{1}", attachment.fileNumber,
                                                                               MimeMapping.GetExtention(attachment.contentType));
                    }

                    attachment.streamId = streamId;
                    attachment.tenant = mailBoxData.TenantId;
                    attachment.user = mailBoxData.UserId;

                    //TODO: Check TenantId and UserId in StorageManager
                    StorageManager.StoreAttachmentWithoutQuota(attachment);
                }

                QuotaEngine.QuotaUsedAdd(quotaAddSize);
            }
            catch
            {
                var storedAttachmentsKeys = attachments
                                            .Where(a => !string.IsNullOrEmpty(a.storedFileUrl))
                                            .Select(MailStoragePathCombiner.GerStoredFilePath)
                                            .ToList();

                if (storedAttachmentsKeys.Any())
                {
                    var storage = StorageFactory.GetMailStorage(mailBoxData.TenantId);

                    storedAttachmentsKeys.ForEach(key => storage.Delete(string.Empty, key));
                }

                Log.InfoFormat("[Failed] StoreAttachments(mailboxId={0}). All message attachments were deleted.", mailBoxData.MailBoxId);

                throw;
            }
        }

        public static MailAttachmentData ToAttachmentData(Attachment attachment)
        {
            if (attachment == null) return null;

            var a = new MailAttachmentData
            {
                fileId = attachment.Id,
                fileName = attachment.Name,
                storedName = attachment.StoredName,
                contentType = attachment.Type,
                size = attachment.Size,
                fileNumber = attachment.FileNumber,
                streamId = attachment.Stream,
                tenant = attachment.Tenant,
                user = attachment.User,
                contentId = attachment.ContentId,
                mailboxId = attachment.MailboxId
            };

            return a;
        }
    }

    public static class AttachmentEngineExtension
    {
        public static DIHelper AddAttachmentEngineService(this DIHelper services)
        {
            services.TryAddScoped<AttachmentEngine>();

            services
                .AddQuotaEngineService()
                .AddChainEngineService()
                .AddIndexEngineService()
                .AddMessageEngineService()
                .AddDaoFactoryService()
                .AddStorageFactoryService()
                .AddStorageManagerService()
                .AddFilesIntegrationService()
                .AddFileSecurityService()
                .AddFileConverterService();

            return services;
        }
    }
}