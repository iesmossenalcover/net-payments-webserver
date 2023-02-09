﻿// <auto-generated />
using System;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace netpaymentswebserver.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.2");

            modelBuilder.Entity("Domain.Entities.Authentication.User", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Firstname")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("HashedPassword")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Lastname")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Username")
                        .IsUnique();

                    b.ToTable("user", "authentication");
                });

            modelBuilder.Entity("Domain.Entities.Authentication.UserClaim", b =>
                {
                    b.Property<string>("Type")
                        .HasColumnType("TEXT");

                    b.Property<long>("UserId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Type", "UserId");

                    b.HasIndex("UserId");

                    b.ToTable("user_claim", "authentication");
                });

            modelBuilder.Entity("Domain.Entities.Events.Event", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("AmipaPrice")
                        .HasColumnType("TEXT");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreationDate")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsAmipa")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("NormalPrice")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("PublishDate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("UnpublishDate")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Code")
                        .IsUnique();

                    b.HasIndex("CreationDate")
                        .IsDescending();

                    b.HasIndex("PublishDate")
                        .IsDescending();

                    b.HasIndex("UnpublishDate")
                        .IsDescending();

                    b.ToTable("event", "event");
                });

            modelBuilder.Entity("Domain.Entities.Events.EventPerson", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("EventId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("ItemId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Paid")
                        .HasColumnType("INTEGER");

                    b.Property<long>("PersonId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("EventId");

                    b.HasIndex("ItemId");

                    b.HasIndex("PersonId", "EventId", "ItemId")
                        .IsUnique();

                    b.ToTable("event_person", "event");
                });

            modelBuilder.Entity("Domain.Entities.Orders.Item", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("OrderId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("OrderId")
                        .IsUnique();

                    b.ToTable("item", "order");
                });

            modelBuilder.Entity("Domain.Entities.Orders.Order", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("Created")
                        .IsDescending();

                    b.ToTable("order", "order");
                });

            modelBuilder.Entity("Domain.Entities.People.Course", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Active")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Active");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.HasIndex("StartDate")
                        .IsDescending();

                    b.ToTable("course", "people");
                });

            modelBuilder.Entity("Domain.Entities.People.Group", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<long?>("ParentId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.HasIndex("ParentId");

                    b.ToTable("group", "people");
                });

            modelBuilder.Entity("Domain.Entities.People.Person", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ContactMail")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("ContactPhone")
                        .HasMaxLength(15)
                        .HasColumnType("TEXT");

                    b.Property<string>("DocumentId")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Surname1")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Surname2")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("DocumentId")
                        .IsUnique();

                    b.HasIndex("Name");

                    b.ToTable("person", "people");
                });

            modelBuilder.Entity("Domain.Entities.People.PersonGroupCourse", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("CourseId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("GroupId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("PersonId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("CourseId");

                    b.HasIndex("GroupId");

                    b.HasIndex("PersonId", "GroupId", "CourseId")
                        .IsUnique();

                    b.ToTable("person_group_course", "people");
                });

            modelBuilder.Entity("Domain.Entities.People.Student", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("AcademicRecordNumber")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Amipa")
                        .HasColumnType("INTEGER");

                    b.Property<long>("PersonId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("PreEnrollment")
                        .HasColumnType("INTEGER");

                    b.Property<string>("SubjectsInfo")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("AcademicRecordNumber")
                        .IsUnique();

                    b.HasIndex("PersonId")
                        .IsUnique();

                    b.ToTable("student", "people");
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

            modelBuilder.Entity("Domain.Entities.Events.EventPerson", b =>
                {
                    b.HasOne("Domain.Entities.Events.Event", "Event")
                        .WithMany()
                        .HasForeignKey("EventId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.Orders.Item", "Item")
                        .WithMany()
                        .HasForeignKey("ItemId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.People.Person", "Person")
                        .WithMany()
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Event");

                    b.Navigation("Item");

                    b.Navigation("Person");
                });

            modelBuilder.Entity("Domain.Entities.Orders.Item", b =>
                {
                    b.HasOne("Domain.Entities.Orders.Order", "Order")
                        .WithMany()
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Order");
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

                    b.Navigation("Group");

                    b.Navigation("Person");
                });

            modelBuilder.Entity("Domain.Entities.People.Student", b =>
                {
                    b.HasOne("Domain.Entities.People.Person", "Person")
                        .WithMany()
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

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
