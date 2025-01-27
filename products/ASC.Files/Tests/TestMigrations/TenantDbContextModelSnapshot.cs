﻿// <auto-generated />
using System;
using ASC.Core.Common.EF.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ASC.Core.Common.Migrations.MySql.TenantDbContextMySql
{
    [DbContext(typeof(MySqlTenantDbContext))]
    partial class TenantDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("ASC.Core.Common.EF.Model.DbCoreSettings", b =>
                {
                    b.Property<int>("Tenant")
                        .HasColumnName("tenant")
                        .HasColumnType("int");

                    b.Property<string>("Id")
                        .HasColumnName("id")
                        .HasColumnType("varchar(128)")
                        .HasAnnotation("MySql:CharSet", "utf8")
                        .HasAnnotation("MySql:Collation", "utf8_general_ci");

                    b.Property<DateTime>("LastModified")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnName("last_modified")
                        .HasColumnType("timestamp")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<byte[]>("Value")
                        .IsRequired()
                        .HasColumnName("value")
                        .HasColumnType("mediumblob");

                    b.HasKey("Tenant", "Id")
                        .HasName("PRIMARY");

                    b.ToTable("core_settings");

                    b.HasData(
                        new
                        {
                            Tenant = -1,
                            Id = "CompanyWhiteLabelSettings",
                            LastModified = new DateTime(2020, 12, 22, 15, 29, 45, 347, DateTimeKind.Utc).AddTicks(9720),
                            Value = new byte[] { 48, 120, 70, 53, 52, 55, 48, 52, 56, 65, 52, 56, 54, 53, 49, 55, 49, 53, 56, 55, 68, 57, 67, 69, 66, 67, 56, 65, 52, 57, 54, 67, 54, 48, 49, 68, 57, 54, 48, 51, 49, 70, 50, 67, 49, 67, 51, 69, 57, 49, 54, 48, 51, 53, 51, 57, 52, 50, 69, 69, 55, 54, 53, 68, 65, 67, 68, 51, 49, 54, 70, 52, 66, 53, 70, 52, 50, 56, 57, 50, 52, 51, 54, 70, 67, 52, 65, 50, 49, 66, 57, 65, 54, 68, 70, 56, 70, 70, 68, 51, 66, 67, 52, 48, 51, 54, 66, 52, 55, 69, 51, 65, 53, 65, 49, 66, 52, 67, 56, 56, 49, 66, 50, 54, 54, 48, 57, 56, 54, 57, 70, 69, 66, 66, 54, 56, 52, 56, 66, 68, 56, 56, 67, 48, 50, 69, 69, 65, 67, 54, 65, 52, 67, 67, 66, 51, 69, 56, 70, 52, 48, 52, 50, 57, 48, 56, 49, 50, 70, 48, 69, 54, 69, 49, 50, 52, 65, 53, 53, 50, 66, 69, 56, 49, 65, 53, 56, 67, 54, 52, 66, 66, 56, 66, 68, 51, 67, 57, 65, 56, 67, 48, 69, 68, 69, 49, 70, 57, 52, 50, 49, 50, 56, 49, 68, 69, 48, 67, 55, 65, 70, 56, 50, 55, 51, 51, 67, 48, 66, 55, 53, 52, 69, 57, 55, 69, 70, 70, 70, 65, 53, 65, 55, 53, 54, 48, 55, 65, 57, 49, 57, 53, 55, 56, 57, 54, 67, 66, 69, 67, 70, 57, 53, 54, 51, 70, 67, 56, 51, 49, 51, 48, 48, 68, 67, 56, 69, 55, 67, 57, 51, 48, 65, 53, 53, 66, 50, 57, 56, 69, 66, 56, 50, 68, 54, 70, 54, 57, 69, 48, 69, 68, 54, 69, 52, 68, 56, 55, 53, 50, 54, 48, 55, 70, 49, 56, 56, 49, 70, 54, 49, 66, 48, 51, 50, 51, 48, 54, 69, 48, 70, 48, 54, 57, 65, 53, 70, 54, 57, 70, 48, 56, 54, 65, 49, 55, 55, 69, 66, 52, 49, 65, 67, 48, 54, 70, 56, 56, 57, 69, 66, 48, 66, 51, 57, 67, 66, 70, 68, 52, 66, 53, 67, 68, 66, 55, 54, 51, 69, 57, 57, 54, 53, 53, 52, 68, 69, 65, 68, 66, 57, 67, 55, 49, 67, 70, 51, 69, 70, 56, 54, 70, 52, 65, 48, 51, 53, 52, 65, 56, 54, 52, 65, 49, 48, 54, 51, 57, 68, 70, 68, 50, 57, 66, 53, 67, 54, 68, 53, 68, 67, 68, 65, 57, 68, 52, 66, 48, 57, 56, 56, 69, 69, 52, 48, 54, 57, 52, 56, 66, 67, 66, 53, 52, 67, 54, 65, 55, 48, 65, 68, 67, 54, 67, 48, 48, 53, 55, 55, 49, 55, 52, 50, 56, 53, 67, 69, 66, 67, 68, 55, 54 }
                        },
                        new
                        {
                            Tenant = -1,
                            Id = "FullTextSearchSettings",
                            LastModified = new DateTime(2020, 12, 22, 15, 29, 45, 348, DateTimeKind.Utc).AddTicks(617),
                            Value = new byte[] { 48, 120, 48, 56, 55, 56, 67, 70, 48, 53, 57, 57, 66, 53, 49, 55, 67, 65, 65, 50, 68, 51, 68, 65, 69, 68, 57, 68, 48, 54, 52, 67, 51, 69, 68, 67, 69, 69, 65, 70, 52, 51, 49, 70, 51, 53, 65, 54, 70, 54, 52, 50, 68, 67, 65, 68, 65, 48, 52, 56, 49, 55, 69, 51, 53, 49, 51, 50, 50, 55, 66, 66, 66, 49, 68, 69, 54, 69, 50, 66, 65, 66, 69, 66, 57, 69, 49, 48, 55, 55, 66, 50, 67, 70, 51, 49, 56, 67, 52, 56, 57, 56, 49, 52, 53, 52, 53, 69, 56, 55, 55, 53, 48, 49, 70, 54, 51, 51, 70, 66, 66, 69, 57, 52, 48, 50, 50, 67, 70, 67, 68, 68, 48, 50, 53, 66, 53, 51, 57, 53, 57, 55, 51, 65, 70, 53, 49, 48, 57, 52, 51, 52, 48, 56, 66, 66, 53, 54, 57, 54, 50, 69, 69, 51, 53, 68, 65, 51, 53, 70, 50, 70, 56, 51, 55, 52, 67, 70, 53, 70, 68, 49, 50, 54, 57, 53, 51, 53, 57, 52, 52, 57, 68, 55, 67, 69, 70, 66, 67, 50, 67, 55, 66, 68, 49, 49, 50, 65, 69, 53, 56, 55, 53, 50, 49, 55, 57, 65, 65, 50, 65, 53, 57, 69, 53, 69, 49, 55, 56, 48, 49, 69, 53, 56, 48, 67, 67, 67, 54, 48, 70, 65, 69, 67, 56, 69, 66, 68, 68, 51, 68, 54, 49, 50, 67, 52, 56, 56, 54, 54, 54, 54, 68, 57, 54, 68, 54, 67, 70, 48, 54, 48, 54, 48, 53, 69, 54, 52, 67, 57, 48, 65, 49, 70, 65, 65, 56, 48, 67, 48 }
                        },
                        new
                        {
                            Tenant = -1,
                            Id = "SmtpSettings",
                            LastModified = new DateTime(2020, 12, 22, 15, 29, 45, 348, DateTimeKind.Utc).AddTicks(639),
                            Value = new byte[] { 48, 120, 70, 48, 53, 50, 69, 48, 57, 48, 65, 49, 65, 51, 55, 53, 48, 68, 65, 68, 67, 68, 52, 69, 57, 57, 54, 49, 68, 65, 48, 52, 65, 65, 53, 49, 69, 70, 48, 49, 57, 55, 69, 50, 67, 48, 54, 50, 51, 67, 70, 49, 50, 67, 53, 56, 51, 56, 66, 70, 65, 52, 48, 65, 57, 66, 52, 56, 66, 65, 69, 70, 67, 66, 69, 51, 55, 49, 53, 56, 55, 55, 51, 49, 68, 55, 69, 51, 68, 67, 57, 69, 55, 67, 54, 48, 48, 57, 55, 52, 50, 70, 57, 69, 52, 49, 53, 68, 53, 54, 68, 66, 48, 70, 48, 65, 69, 48, 56, 69, 51, 50, 70, 56, 57, 48, 52, 66, 50, 67, 52, 52, 49, 67, 67, 54, 53, 55, 67, 54, 52, 53, 52, 51, 69, 65, 69, 69, 50, 54, 50, 48, 52, 52, 65, 50, 56, 66, 52, 51, 51, 53, 68, 67, 66, 48, 70, 48, 67, 52, 69, 57, 52, 48, 49, 68, 56, 57, 49, 70, 65, 48, 54, 51, 54, 57, 70, 57, 56, 52, 67, 65, 50, 68, 52, 55, 53, 67, 56, 54, 67, 50, 51, 55, 57, 49, 55, 57, 54, 49, 67, 53, 56, 50, 55, 55, 54, 57, 56, 51, 49, 53, 56, 53, 50, 51, 48, 65, 54, 54, 65, 67, 55, 55, 56, 55, 69, 54, 70, 66, 53, 54, 70, 68, 51, 69, 51, 55, 51, 56, 57, 50, 54, 55, 65, 52, 54, 65 }
                        });
                });

            modelBuilder.Entity("ASC.Core.Common.EF.Model.DbTenant", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("int");

                    b.Property<string>("Alias")
                        .IsRequired()
                        .HasColumnName("alias")
                        .HasColumnType("varchar(100)")
                        .HasAnnotation("MySql:CharSet", "utf8")
                        .HasAnnotation("MySql:Collation", "utf8_general_ci");

                    b.Property<bool>("Calls")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("calls")
                        .HasColumnType("tinyint(1)")
                        .HasDefaultValueSql("true");

                    b.Property<DateTime>("CreationDateTime")
                        .HasColumnName("creationdatetime")
                        .HasColumnType("datetime");

                    b.Property<int?>("Industry")
                        .HasColumnName("industry")
                        .HasColumnType("int");

                    b.Property<string>("Language")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnName("language")
                        .HasColumnType("char(10)")
                        .HasDefaultValueSql("'en-US'")
                        .HasAnnotation("MySql:CharSet", "utf8")
                        .HasAnnotation("MySql:Collation", "utf8_general_ci");

                    b.Property<DateTime>("LastModified")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("last_modified")
                        .HasColumnType("timestamp")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("MappedDomain")
                        .HasColumnName("mappeddomain")
                        .HasColumnType("varchar(100)")
                        .HasAnnotation("MySql:CharSet", "utf8")
                        .HasAnnotation("MySql:Collation", "utf8_general_ci");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("name")
                        .HasColumnType("varchar(255)")
                        .HasAnnotation("MySql:CharSet", "utf8")
                        .HasAnnotation("MySql:Collation", "utf8_general_ci");

                    b.Property<string>("OwnerId")
                        .IsRequired()
                        .HasColumnName("owner_id")
                        .HasColumnType("varchar(38)")
                        .HasAnnotation("MySql:CharSet", "utf8")
                        .HasAnnotation("MySql:Collation", "utf8_general_ci");

                    b.Property<string>("PaymentId")
                        .HasColumnName("payment_id")
                        .HasColumnType("varchar(38)")
                        .HasAnnotation("MySql:CharSet", "utf8")
                        .HasAnnotation("MySql:Collation", "utf8_general_ci");

                    b.Property<bool>("Public")
                        .HasColumnName("public")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("PublicVisibleProducts")
                        .HasColumnName("publicvisibleproducts")
                        .HasColumnType("varchar(1024)")
                        .HasAnnotation("MySql:CharSet", "utf8")
                        .HasAnnotation("MySql:Collation", "utf8_general_ci");

                    b.Property<bool>("Spam")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("spam")
                        .HasColumnType("tinyint(1)")
                        .HasDefaultValueSql("true");

                    b.Property<int>("Status")
                        .HasColumnName("status")
                        .HasColumnType("int");

                    b.Property<DateTime?>("StatusChanged")
                        .HasColumnName("statuschanged")
                        .HasColumnType("datetime");

                    b.Property<string>("TimeZone")
                        .HasColumnName("timezone")
                        .HasColumnType("varchar(50)")
                        .HasAnnotation("MySql:CharSet", "utf8")
                        .HasAnnotation("MySql:Collation", "utf8_general_ci");

                    b.Property<string>("TrustedDomains")
                        .HasColumnName("trusteddomains")
                        .HasColumnType("varchar(1024)")
                        .HasAnnotation("MySql:CharSet", "utf8")
                        .HasAnnotation("MySql:Collation", "utf8_general_ci");

                    b.Property<int>("TrustedDomainsEnabled")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("trusteddomainsenabled")
                        .HasColumnType("int")
                        .HasDefaultValueSql("'1'");

                    b.Property<int>("Version")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("version")
                        .HasColumnType("int")
                        .HasDefaultValueSql("'2'");

                    b.Property<DateTime?>("Version_Changed")
                        .HasColumnName("version_changed")
                        .HasColumnType("datetime");

                    b.HasKey("Id");

                    b.HasIndex("LastModified")
                        .HasName("last_modified");

                    b.HasIndex("MappedDomain")
                        .HasName("mappeddomain");

                    b.HasIndex("Version")
                        .HasName("version");

                    b.ToTable("tenants_tenants");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Alias = "localhost",
                            Calls = false,
                            CreationDateTime = new DateTime(2020, 12, 22, 15, 29, 45, 343, DateTimeKind.Utc).AddTicks(7822),
                            LastModified = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Name = "Web Office",
                            OwnerId = "66faa6e4-f133-11ea-b126-00ffeec8b4ef",
                            Public = false,
                            Spam = false,
                            Status = 0,
                            TrustedDomainsEnabled = 0,
                            Version = 0
                        });
                });

            modelBuilder.Entity("ASC.Core.Common.EF.Model.DbTenantForbiden", b =>
                {
                    b.Property<string>("Address")
                        .HasColumnName("address")
                        .HasColumnType("varchar(50)")
                        .HasAnnotation("MySql:CharSet", "utf8")
                        .HasAnnotation("MySql:Collation", "utf8_general_ci");

                    b.HasKey("Address")
                        .HasName("PRIMARY");

                    b.ToTable("tenants_forbiden");

                    b.HasData(
                        new
                        {
                            Address = "controlpanel"
                        },
                        new
                        {
                            Address = "localhost"
                        });
                });

            modelBuilder.Entity("ASC.Core.Common.EF.Model.DbTenantPartner", b =>
                {
                    b.Property<int>("TenantId")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("tenant_id")
                        .HasColumnType("int");

                    b.Property<string>("AffiliateId")
                        .HasColumnName("affiliate_id")
                        .HasColumnType("varchar(50)")
                        .HasAnnotation("MySql:CharSet", "utf8")
                        .HasAnnotation("MySql:Collation", "utf8_general_ci");

                    b.Property<string>("Campaign")
                        .HasColumnName("campaign")
                        .HasColumnType("varchar(50)")
                        .HasAnnotation("MySql:CharSet", "utf8")
                        .HasAnnotation("MySql:Collation", "utf8_general_ci");

                    b.Property<string>("PartnerId")
                        .HasColumnName("partner_id")
                        .HasColumnType("varchar(36)")
                        .HasAnnotation("MySql:CharSet", "utf8")
                        .HasAnnotation("MySql:Collation", "utf8_general_ci");

                    b.Property<int?>("TenantId1")
                        .HasColumnType("int");

                    b.HasKey("TenantId")
                        .HasName("PRIMARY");

                    b.HasIndex("TenantId1");

                    b.ToTable("tenants_partners");
                });

            modelBuilder.Entity("ASC.Core.Common.EF.Model.DbTenantVersion", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("int");

                    b.Property<int>("DefaultVersion")
                        .HasColumnName("default_version")
                        .HasColumnType("int");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnName("url")
                        .HasColumnType("varchar(64)")
                        .HasAnnotation("MySql:CharSet", "utf8")
                        .HasAnnotation("MySql:Collation", "utf8_general_ci");

                    b.Property<string>("Version")
                        .IsRequired()
                        .HasColumnName("version")
                        .HasColumnType("varchar(64)")
                        .HasAnnotation("MySql:CharSet", "utf8")
                        .HasAnnotation("MySql:Collation", "utf8_general_ci");

                    b.Property<bool>("Visible")
                        .HasColumnName("visible")
                        .HasColumnType("tinyint(1)");

                    b.HasKey("Id");

                    b.ToTable("tenants_version");
                });

            modelBuilder.Entity("ASC.Core.Common.EF.Model.TenantIpRestrictions", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("int");

                    b.Property<string>("Ip")
                        .IsRequired()
                        .HasColumnName("ip")
                        .HasColumnType("varchar(50)")
                        .HasAnnotation("MySql:CharSet", "utf8")
                        .HasAnnotation("MySql:Collation", "utf8_general_ci");

                    b.Property<int>("Tenant")
                        .HasColumnName("tenant")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("Tenant")
                        .HasName("tenant");

                    b.ToTable("tenants_iprestrictions");
                });

            modelBuilder.Entity("ASC.Core.Common.EF.User", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("varchar(38)")
                        .HasAnnotation("MySql:CharSet", "utf8")
                        .HasAnnotation("MySql:Collation", "utf8_general_ci");

                    b.Property<int>("ActivationStatus")
                        .HasColumnName("activation_status")
                        .HasColumnType("int");

                    b.Property<DateTime?>("Birthdate")
                        .HasColumnName("bithdate")
                        .HasColumnType("datetime");

                    b.Property<string>("Contacts")
                        .HasColumnName("contacts")
                        .HasColumnType("varchar(1024)")
                        .HasAnnotation("MySql:CharSet", "utf8")
                        .HasAnnotation("MySql:Collation", "utf8_general_ci");

                    b.Property<DateTime>("CreateOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("create_on")
                        .HasColumnType("timestamp")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("Culture")
                        .HasColumnName("culture")
                        .HasColumnType("varchar(20)")
                        .HasAnnotation("MySql:CharSet", "utf8")
                        .HasAnnotation("MySql:Collation", "utf8_general_ci");

                    b.Property<string>("Email")
                        .HasColumnName("email")
                        .HasColumnType("varchar(255)")
                        .HasAnnotation("MySql:CharSet", "utf8")
                        .HasAnnotation("MySql:Collation", "utf8_general_ci");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnName("firstname")
                        .HasColumnType("varchar(64)")
                        .HasAnnotation("MySql:CharSet", "utf8")
                        .HasAnnotation("MySql:Collation", "utf8_general_ci");

                    b.Property<DateTime>("LastModified")
                        .HasColumnName("last_modified")
                        .HasColumnType("datetime");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnName("lastname")
                        .HasColumnType("varchar(64)")
                        .HasAnnotation("MySql:CharSet", "utf8")
                        .HasAnnotation("MySql:Collation", "utf8_general_ci");

                    b.Property<string>("Location")
                        .HasColumnName("location")
                        .HasColumnType("varchar(255)")
                        .HasAnnotation("MySql:CharSet", "utf8")
                        .HasAnnotation("MySql:Collation", "utf8_general_ci");

                    b.Property<string>("Notes")
                        .HasColumnName("notes")
                        .HasColumnType("varchar(512)")
                        .HasAnnotation("MySql:CharSet", "utf8")
                        .HasAnnotation("MySql:Collation", "utf8_general_ci");

                    b.Property<string>("Phone")
                        .HasColumnName("phone")
                        .HasColumnType("varchar(255)")
                        .HasAnnotation("MySql:CharSet", "utf8")
                        .HasAnnotation("MySql:Collation", "utf8_general_ci");

                    b.Property<int>("PhoneActivation")
                        .HasColumnName("phone_activation")
                        .HasColumnType("int");

                    b.Property<bool>("Removed")
                        .HasColumnName("removed")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool?>("Sex")
                        .HasColumnName("sex")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Sid")
                        .HasColumnName("sid")
                        .HasColumnType("varchar(512)")
                        .HasAnnotation("MySql:CharSet", "utf8")
                        .HasAnnotation("MySql:Collation", "utf8_general_ci");

                    b.Property<string>("SsoNameId")
                        .HasColumnName("sso_name_id")
                        .HasColumnType("varchar(512)")
                        .HasAnnotation("MySql:CharSet", "utf8")
                        .HasAnnotation("MySql:Collation", "utf8_general_ci");

                    b.Property<string>("SsoSessionId")
                        .HasColumnName("sso_session_id")
                        .HasColumnType("varchar(512)")
                        .HasAnnotation("MySql:CharSet", "utf8")
                        .HasAnnotation("MySql:Collation", "utf8_general_ci");

                    b.Property<int>("Status")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("status")
                        .HasColumnType("int")
                        .HasDefaultValueSql("'1'");

                    b.Property<int>("Tenant")
                        .HasColumnName("tenant")
                        .HasColumnType("int");

                    b.Property<DateTime?>("TerminatedDate")
                        .HasColumnName("terminateddate")
                        .HasColumnType("datetime");

                    b.Property<string>("Title")
                        .HasColumnName("title")
                        .HasColumnType("varchar(64)")
                        .HasAnnotation("MySql:CharSet", "utf8")
                        .HasAnnotation("MySql:Collation", "utf8_general_ci");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnName("username")
                        .HasColumnType("varchar(255)")
                        .HasAnnotation("MySql:CharSet", "utf8")
                        .HasAnnotation("MySql:Collation", "utf8_general_ci");

                    b.Property<DateTime?>("WorkFromDate")
                        .HasColumnName("workfromdate")
                        .HasColumnType("datetime");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .HasName("email");

                    b.HasIndex("LastModified")
                        .HasName("last_modified");

                    b.HasIndex("Tenant", "UserName")
                        .HasName("username");

                    b.ToTable("core_user");

                    b.HasData(
                        new
                        {
                            Id = "66faa6e4-f133-11ea-b126-00ffeec8b4ef",
                            ActivationStatus = 0,
                            CreateOn = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Email = "",
                            FirstName = "Administrator",
                            LastModified = new DateTime(2020, 12, 22, 15, 29, 45, 332, DateTimeKind.Utc).AddTicks(2191),
                            LastName = "",
                            PhoneActivation = 0,
                            Removed = false,
                            Status = 1,
                            Tenant = 1,
                            UserName = "administrator",
                            WorkFromDate = new DateTime(2020, 12, 22, 15, 29, 45, 332, DateTimeKind.Utc).AddTicks(1292)
                        },
                        new
                        {
                            Id = "005bb3ff-7de3-47d2-9b3d-61b9ec8a76a5",
                            ActivationStatus = 0,
                            CreateOn = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Email = "test@gmail.com",
                            FirstName = "Test",
                            LastModified = new DateTime(2020, 12, 22, 15, 29, 45, 332, DateTimeKind.Utc).AddTicks(3070),
                            LastName = "User",
                            PhoneActivation = 0,
                            Removed = false,
                            Status = 1,
                            Tenant = 1,
                            UserName = "TestUser",
                            WorkFromDate = new DateTime(2020, 12, 22, 15, 29, 45, 332, DateTimeKind.Utc).AddTicks(3053)
                        });
                });

            modelBuilder.Entity("ASC.Core.Common.EF.UserGroup", b =>
                {
                    b.Property<int>("Tenant")
                        .HasColumnName("tenant")
                        .HasColumnType("int");

                    b.Property<string>("UserId")
                        .HasColumnName("userid")
                        .HasColumnType("varchar(38)")
                        .HasAnnotation("MySql:CharSet", "utf8")
                        .HasAnnotation("MySql:Collation", "utf8_general_ci");

                    b.Property<string>("GroupId")
                        .HasColumnName("groupid")
                        .HasColumnType("varchar(38)")
                        .HasAnnotation("MySql:CharSet", "utf8")
                        .HasAnnotation("MySql:Collation", "utf8_general_ci");

                    b.Property<int>("RefType")
                        .HasColumnName("ref_type")
                        .HasColumnType("int");

                    b.Property<DateTime>("LastModified")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("last_modified")
                        .HasColumnType("timestamp")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<bool>("Removed")
                        .HasColumnName("removed")
                        .HasColumnType("tinyint(1)");

                    b.HasKey("Tenant", "UserId", "GroupId", "RefType")
                        .HasName("PRIMARY");

                    b.HasIndex("LastModified")
                        .HasName("last_modified");

                    b.HasIndex("UserId");

                    b.ToTable("core_usergroup");
                });

            modelBuilder.Entity("ASC.Core.Common.EF.UserSecurity", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnName("userid")
                        .HasColumnType("varchar(38)")
                        .HasAnnotation("MySql:CharSet", "utf8")
                        .HasAnnotation("MySql:Collation", "utf8_general_ci");

                    b.Property<DateTime?>("LastModified")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("timestamp")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("PwdHash")
                        .HasColumnName("pwdhash")
                        .HasColumnType("varchar(512)")
                        .HasAnnotation("MySql:CharSet", "utf8")
                        .HasAnnotation("MySql:Collation", "utf8_general_ci");

                    b.Property<string>("PwdHashSha512")
                        .HasColumnName("pwdhashsha512")
                        .HasColumnType("varchar(512)")
                        .HasAnnotation("MySql:CharSet", "utf8")
                        .HasAnnotation("MySql:Collation", "utf8_general_ci");

                    b.Property<int>("Tenant")
                        .HasColumnName("tenant")
                        .HasColumnType("int");

                    b.HasKey("UserId")
                        .HasName("PRIMARY");

                    b.HasIndex("PwdHash")
                        .HasName("pwdhash");

                    b.HasIndex("Tenant")
                        .HasName("tenant");

                    b.ToTable("core_usersecurity");

                    b.HasData(
                        new
                        {
                            UserId = "66faa6e4-f133-11ea-b126-00ffeec8b4ef",
                            LastModified = new DateTime(2020, 12, 22, 15, 29, 45, 351, DateTimeKind.Utc).AddTicks(8712),
                            PwdHash = "vLFfghR5tNV3K9DKhmwArV+SbjWAcgZZzIDTnJ0JgCo=",
                            PwdHashSha512 = "USubvPlB+ogq0Q1trcSupg==",
                            Tenant = 1
                        },
                        new
                        {
                            UserId = "005bb3ff-7de3-47d2-9b3d-61b9ec8a76a5",
                            LastModified = new DateTime(2020, 12, 22, 15, 29, 45, 351, DateTimeKind.Utc).AddTicks(9655),
                            PwdHash = "vLFfghR5tNV3K9DKhmwArV+SbjWAcgZZzIDTnJ0JgCo=",
                            PwdHashSha512 = "USubvPlB+ogq0Q1trcSupg==",
                            Tenant = 1
                        });
                });

            modelBuilder.Entity("ASC.Core.Common.EF.Model.DbTenantPartner", b =>
                {
                    b.HasOne("ASC.Core.Common.EF.Model.DbTenant", "Tenant")
                        .WithMany()
                        .HasForeignKey("TenantId1");
                });

            modelBuilder.Entity("ASC.Core.Common.EF.UserGroup", b =>
                {
                    b.HasOne("ASC.Core.Common.EF.User", null)
                        .WithMany("Groups")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ASC.Core.Common.EF.UserSecurity", b =>
                {
                    b.HasOne("ASC.Core.Common.EF.User", null)
                        .WithOne("UserSecurity")
                        .HasForeignKey("ASC.Core.Common.EF.UserSecurity", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
