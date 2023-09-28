﻿// <auto-generated />
using System;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace netpaymentswebserver.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:CollationDefinition:no_accent", "und-u-ks-level1-kc-true,und-u-ks-level1-kc-true,icu,False")
                .HasAnnotation("ProductVersion", "7.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Domain.Entities.Authentication.OAuthUser", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("OAuthProviderCode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Subject")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.HasIndex("Subject", "OAuthProviderCode")
                        .IsUnique();

                    b.ToTable("oauth_user", "main");
                });

            modelBuilder.Entity("Domain.Entities.Authentication.User", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("Firstname")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("HashedPassword")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Lastname")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.HasKey("Id");

                    b.HasIndex("Username")
                        .IsUnique();

                    b.ToTable("user", "main");
                });

            modelBuilder.Entity("Domain.Entities.Authentication.UserClaim", b =>
                {
                    b.Property<string>("Type")
                        .HasColumnType("text");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Type", "UserId");

                    b.HasIndex("UserId");

                    b.ToTable("user_claim", "main");
                });

            modelBuilder.Entity("Domain.Entities.Configuration.AppConfig", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<bool>("DisplayEnrollment")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.ToTable("app_config", "main");
                });

            modelBuilder.Entity("Domain.Entities.Events.Event", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<bool>("Amipa")
                        .HasColumnType("boolean");

                    b.Property<decimal>("AmipaPrice")
                        .HasColumnType("numeric");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.Property<long>("CourseId")
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset>("CreationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("Enrollment")
                        .HasColumnType("boolean");

                    b.Property<long>("MaxQuantity")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasDefaultValue(1L);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("Price")
                        .HasColumnType("numeric");

                    b.Property<DateTimeOffset>("PublishDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset?>("UnpublishDate")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("Code")
                        .IsUnique();

                    b.HasIndex("CourseId");

                    b.HasIndex("CreationDate")
                        .IsDescending();

                    b.HasIndex("PublishDate")
                        .IsDescending();

                    b.HasIndex("UnpublishDate")
                        .IsDescending();

                    b.ToTable("event", "main");
                });

            modelBuilder.Entity("Domain.Entities.Events.EventPerson", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<DateTimeOffset?>("DatePaid")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long>("EventId")
                        .HasColumnType("bigint");

                    b.Property<long?>("OrderId")
                        .HasColumnType("bigint");

                    b.Property<bool>("Paid")
                        .HasColumnType("boolean");

                    b.Property<bool>("PaidAsAmipa")
                        .HasColumnType("boolean");

                    b.Property<long>("PersonId")
                        .HasColumnType("bigint");

                    b.Property<long>("Quantity")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasDefaultValue(1L);

                    b.HasKey("Id");

                    b.HasIndex("EventId");

                    b.HasIndex("OrderId");

                    b.HasIndex("PersonId", "EventId", "OrderId")
                        .IsUnique();

                    b.ToTable("event_person", "main");
                });

            modelBuilder.Entity("Domain.Entities.GoogleApi.OuGroupRelation", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("ActiveOU")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("ChangePasswordNextSignIn")
                        .HasColumnType("boolean");

                    b.Property<long>("GroupId")
                        .HasColumnType("bigint");

                    b.Property<string>("GroupMail")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("OldOU")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("UpdatePassword")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.HasIndex("GroupId")
                        .IsDescending();

                    b.ToTable("uo_group_relation", "main");
                });

            modelBuilder.Entity("Domain.Entities.Jobs.Job", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<DateTimeOffset?>("End")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long?>("LogId")
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset>("Start")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("LogId");

                    b.HasIndex("Type", "Status")
                        .IsDescending();

                    b.ToTable("job", "main");
                });

            modelBuilder.Entity("Domain.Entities.Logs.LogStoreInfo", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("Info")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("log_info", "main");
                });

            modelBuilder.Entity("Domain.Entities.Orders.Order", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset>("PaidDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long>("PersonId")
                        .HasColumnType("bigint");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("Created")
                        .IsDescending();

                    b.HasIndex("PersonId");

                    b.HasIndex("PaidDate", "Status")
                        .IsDescending();

                    b.ToTable("order", "main");
                });

            modelBuilder.Entity("Domain.Entities.People.Course", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<bool>("Active")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset>("EndDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<DateTimeOffset>("StartDate")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("Active");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.HasIndex("StartDate")
                        .IsDescending();

                    b.ToTable("course", "main");
                });

            modelBuilder.Entity("Domain.Entities.People.Group", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<DateTimeOffset>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<long?>("ParentId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.HasIndex("ParentId");

                    b.ToTable("group", "main");
                });

            modelBuilder.Entity("Domain.Entities.People.Person", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long?>("AcademicRecordNumber")
                        .HasColumnType("bigint");

                    b.Property<string>("ContactMail")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("ContactPhone")
                        .HasMaxLength(15)
                        .HasColumnType("character varying(15)");

                    b.Property<string>("DocumentId")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Surname1")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Surname2")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("AcademicRecordNumber")
                        .IsUnique();

                    b.HasIndex("ContactMail")
                        .IsUnique();

                    b.HasIndex("DocumentId")
                        .IsUnique();

                    b.HasIndex("Name");

                    b.HasIndex("Surname1");

                    b.HasIndex("Surname2");

                    b.ToTable("person", "main");
                });

            modelBuilder.Entity("Domain.Entities.People.PersonGroupCourse", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<bool>("Amipa")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset?>("AmipaDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long>("CourseId")
                        .HasColumnType("bigint");

                    b.Property<bool>("Enrolled")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset?>("EnrolledDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long?>("EnrollmentEventId")
                        .HasColumnType("bigint");

                    b.Property<long>("GroupId")
                        .HasColumnType("bigint");

                    b.Property<long>("PersonId")
                        .HasColumnType("bigint");

                    b.Property<string>("SubjectsInfo")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("CourseId");

                    b.HasIndex("EnrollmentEventId");

                    b.HasIndex("GroupId");

                    b.HasIndex("PersonId", "GroupId", "CourseId")
                        .IsUnique();

                    b.ToTable("person_group_course", "main");
                });

            modelBuilder.Entity("Domain.Entities.Authentication.OAuthUser", b =>
                {
                    b.HasOne("Domain.Entities.Authentication.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Domain.Entities.Authentication.UserClaim", b =>
                {
                    b.HasOne("Domain.Entities.Authentication.User", "User")
                        .WithMany("UserClaims")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Domain.Entities.Events.Event", b =>
                {
                    b.HasOne("Domain.Entities.People.Course", "Course")
                        .WithMany()
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Course");
                });

            modelBuilder.Entity("Domain.Entities.Events.EventPerson", b =>
                {
                    b.HasOne("Domain.Entities.Events.Event", "Event")
                        .WithMany()
                        .HasForeignKey("EventId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.Orders.Order", "Order")
                        .WithMany()
                        .HasForeignKey("OrderId");

                    b.HasOne("Domain.Entities.People.Person", "Person")
                        .WithMany()
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Event");

                    b.Navigation("Order");

                    b.Navigation("Person");
                });

            modelBuilder.Entity("Domain.Entities.GoogleApi.OuGroupRelation", b =>
                {
                    b.HasOne("Domain.Entities.People.Group", "Group")
                        .WithMany()
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");
                });

            modelBuilder.Entity("Domain.Entities.Jobs.Job", b =>
                {
                    b.HasOne("Domain.Entities.Logs.LogStoreInfo", "Log")
                        .WithMany()
                        .HasForeignKey("LogId");

                    b.Navigation("Log");
                });

            modelBuilder.Entity("Domain.Entities.Orders.Order", b =>
                {
                    b.HasOne("Domain.Entities.People.Person", "Person")
                        .WithMany()
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Person");
                });

            modelBuilder.Entity("Domain.Entities.People.Group", b =>
                {
                    b.HasOne("Domain.Entities.People.Group", "Parent")
                        .WithMany()
                        .HasForeignKey("ParentId");

                    b.Navigation("Parent");
                });

            modelBuilder.Entity("Domain.Entities.People.PersonGroupCourse", b =>
                {
                    b.HasOne("Domain.Entities.People.Course", "Course")
                        .WithMany()
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.Events.Event", "EnrollmentEvent")
                        .WithMany()
                        .HasForeignKey("EnrollmentEventId");

                    b.HasOne("Domain.Entities.People.Group", "Group")
                        .WithMany()
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.People.Person", "Person")
                        .WithMany()
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Course");

                    b.Navigation("EnrollmentEvent");

                    b.Navigation("Group");

                    b.Navigation("Person");
                });

            modelBuilder.Entity("Domain.Entities.Authentication.User", b =>
                {
                    b.Navigation("UserClaims");
                });
#pragma warning restore 612, 618
        }
    }
}
