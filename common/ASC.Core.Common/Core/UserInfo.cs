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
using System.Globalization;
using System.Text;

using ASC.Common;
using ASC.Common.Caching;
using ASC.Notify.Recipients;

using Confluent.Kafka;

using Google.Protobuf;
//using Google.Protobuf.WellKnownTypes;

namespace ASC.Core.Users
{
    [Serializable]
    public sealed partial class UserInfo : IDirectRecipient//, ICloneable
    {
        partial void OnConstruction()
        {
            Status = EmployeeStatus.Active;
            ActivationStatus = (int)EmployeeActivationStatus.NotActivated;
            LastModified = DateTime.UtcNow;
        }


        public Guid ID { 
            get
            {
                return IDProto.FromByteString();
            }
            set
            {
                IDProto = value.ToByteString();
            }
        }

        //public string FirstName { get; set; }

        //public string LastName { get; set; }

        //public string UserName { get; set; }

        public DateTime? BirthDate
        {
            get
            {
                return BirthDateProto.ToDateTime();
            }
            set
            {
                BirthDateProto = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(value ?? default);
            }
        }

        public bool? Sex { 
            get
            {
                if (SexIsNull) return null;
                else return SexProto;
            }
            set
            {
                SexIsNull = !value.HasValue;
                SexProto = value ?? default;
            }
        }

        public EmployeeStatus Status {
            get
            {
                return (EmployeeStatus)StatusProto;
            }
            set
            {
                StatusProto = (int)value;
            }
        }

        public EmployeeActivationStatus ActivationStatus
        {
            get
            {
                return (EmployeeActivationStatus)ActivationStatusProto;
            }
            set
            {
                ActivationStatusProto = (int)value;
            }
        }

        public DateTime? TerminatedDate
        {
            get
            {
                return TerminatedDateProto.ToDateTime();
            }
            set
            {
                TerminatedDateProto = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(value??default);
            }
        }

        //public string Title { get; set; }

        public DateTime? WorkFromDate
        {
            get
            {
                return WorkFromDateProto.ToDateTime();
            }
            set
            {
                WorkFromDateProto = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(value ?? default);
            }
        }

        //public string Email { get; set; }

        //private string contacts;
        //public string Contacts
        //{
        //    get => contacts;
        //    set
        //    {
        //        contacts = value;
        //        ContactsFromString(contacts);
        //    }
        //}

        //public List<string> ContactsList { get; set; }

        //public string Location { get; set; }

        //public string Notes { get; set; }

        //public bool Removed { get; set; }

        public DateTime LastModified 
        {
            get
            {
                return LastModifiedProto.ToDateTime();
            }
            set
            {
                LastModifiedProto = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(value); 
            }
        }

        //public int Tenant { get; set; }

        public bool IsActive
        {
            get { return ((EmployeeActivationStatus)ActivationStatus).HasFlag(EmployeeActivationStatus.Activated); }
        }

        //public string CultureName { get; set; }

        //public string MobilePhone { get; set; }

        public MobilePhoneActivationStatus MobilePhoneActivationStatus
        {
            get
            {
                return (MobilePhoneActivationStatus)MobilePhoneActivationStatusProto;
            }
            set
            {
                MobilePhoneActivationStatusProto = (int)value;
            }
        }

        //public string Sid { get; set; } // LDAP user identificator

        //public string SsoNameId { get; set; } // SSO SAML user identificator

        //public string SsoSessionId { get; set; } // SSO SAML user session identificator

        public DateTime CreateDate
        {
            get
            {
                return LastModifiedProto.ToDateTime();
            }
            set
            {
                LastModifiedProto = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(value);
            }
        }

        //public override string ToString()
        //{
        //    return string.Format("{0} {1}", FirstName, LastName).Trim();
        //}

        //public override int GetHashCode()
        //{
        //    return ID.GetHashCode();
        //}

        //public override bool Equals(object obj)
        //{
        //    return obj is UserInfo ui && ID.Equals(ui.ID);
        //}

        //public bool Equals(UserInfo obj)
        //{
        //    return obj != null && ID.Equals(obj.ID);
        //}

        public CultureInfo GetCulture()
        {
            return string.IsNullOrEmpty(CultureName) ? CultureInfo.CurrentCulture : CultureInfo.GetCultureInfo(CultureName);
        }


        string[] IDirectRecipient.Addresses
        {
            get { return !string.IsNullOrEmpty(Email) ? new[] { Email } : new string[0]; }
        }

        public bool CheckActivation
        {
            get { return !IsActive; /*if user already active we don't need activation*/ }
        }

        string IRecipient.ID
        {
            get { return ID.ToString(); }
        }

        string IRecipient.Name
        {
            get { return this.ToString(); }
        }

        //public object Clone()
        //{
        //    return MemberwiseClone();
        //}


        internal string ContactsToString()
        {
            if (ContactsList == null || ContactsList.Count == 0) return null;
            var sBuilder = new StringBuilder();
            foreach (var contact in ContactsList)
            {
                sBuilder.AppendFormat("{0}|", contact);
            }
            return sBuilder.ToString();
        }

        internal UserInfo ContactsFromString(string contacts)
        {
            if (string.IsNullOrEmpty(contacts)) return this;

            ContactsList.Clear();

            ContactsList.AddRange(contacts.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries));

            return this;
        }

        //public static implicit operator UserInfo(UserInfoCache cache)
        //{
        //    var result = new UserInfo
        //    {
        //        ActivationStatus = (EmployeeActivationStatus)cache.ActivationStatus,
        //        BirthDate = cache.BirthDate.ToDateTime(),
        //        Contacts = cache.Contacts,
        //        CreateDate = cache.CreateDate.ToDateTime(),
        //        CultureName = cache.CultureName,
        //        Email = cache.Email,
        //        FirstName = cache.FirstName,
        //        ID = cache.ID.FromByteString(),
        //        LastModified = cache.LastModified.ToDateTime(),
        //        LastName = cache.LastName,
        //        Location = cache.Location,
        //        MobilePhone = cache.MobilePhone,
        //        MobilePhoneActivationStatus = (MobilePhoneActivationStatus)cache.MobilePhoneActivationStatus,
        //        Notes = cache.Notes,
        //        Removed = cache.Removed,
        //        Sex = cache.Sex,
        //        Sid = cache.Sid,
        //        SsoNameId = cache.SsoNameId,
        //        SsoSessionId = cache.SsoSessionId,
        //        Status = (EmployeeStatus)cache.Status,
        //        Tenant = cache.Tenant,
        //        TerminatedDate = cache.TerminatedDate.ToDateTime(),
        //        Title = cache.Title,
        //        UserName = cache.UserName,
        //        WorkFromDate = cache.WorkFromDate.ToDateTime()
        //    };
        //    result.ContactsList = new List<string>(cache.ContactsList.Count);
        //    result.ContactsList.AddRange(cache.ContactsList);
        //    return result;
        //}

        //public static implicit operator UserInfoCache(UserInfo origin)
        //{
        //    var result = new UserInfoCache
        //    {
        //        ActivationStatus = (int)origin.ActivationStatus,
        //        BirthDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(origin.BirthDate ?? default), //TODO: Design nullable Protobuf Timestamp
        //        Contacts = origin.Contacts,
        //        CreateDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(origin.CreateDate),
        //        CultureName = origin.CultureName,
        //        Email = origin.Email,
        //        FirstName = origin.FirstName,
        //        ID = origin.ID.ToByteString(),
        //        LastModified = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(origin.LastModified),
        //        LastName = origin.LastName,
        //        Location = origin.Location,
        //        MobilePhone = origin.MobilePhone,
        //        MobilePhoneActivationStatus = (int)origin.MobilePhoneActivationStatus,
        //        Notes = origin.Notes,
        //        Removed = origin.Removed,
        //        Sex = (bool)origin.Sex,
        //        Sid = origin.Sid,
        //        SsoNameId = origin.SsoNameId,
        //        SsoSessionId = origin.SsoSessionId,
        //        Status = (int)origin.Status,
        //        Tenant = origin.Tenant,
        //        TerminatedDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(origin.TerminatedDate ?? default),
        //        Title = origin.Title,
        //        UserName = origin.UserName,
        //        WorkFromDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(origin.WorkFromDate ?? default)
        //    };
        //    result.ContactsList.AddRange(origin.ContactsList);
        //    return result;
        //}
        //public byte[] Serialize(UserInfo data, SerializationContext context)
        //{
        //    return ((UserInfo)data).ToByteArray();
        //}

        //public UserInfo Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        //{
        //    var parser = new MessageParser<UserInfo>(() => new UserInfo());
        //    return (UserInfo)(parser.ParseFrom(data.ToArray()));
        //}
    }
}