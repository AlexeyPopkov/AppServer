﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASC.Mail.Core.Dao.Entities
{
    [Table("mail_mailbox_provider")]
    public partial class MailMailboxProvider
    {
        [Key]
        [Column("id", TypeName = "int(11)")]
        public int Id { get; set; }
        [Required]
        [Column("name", TypeName = "varchar(255)")]
        public string Name { get; set; }
        [Column("display_name", TypeName = "varchar(255)")]
        public string DisplayName { get; set; }
        [Column("display_short_name", TypeName = "varchar(255)")]
        public string DisplayShortName { get; set; }
        [Column("documentation", TypeName = "varchar(255)")]
        public string Documentation { get; set; }
    }
}