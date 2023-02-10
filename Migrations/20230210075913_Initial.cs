using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace netpaymentswebserver.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "people");

            migrationBuilder.EnsureSchema(
                name: "event");

            migrationBuilder.EnsureSchema(
                name: "order");

            migrationBuilder.EnsureSchema(
                name: "authentication");

            migrationBuilder.CreateTable(
                name: "course",
                schema: "people",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Active = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_course", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "event",
                schema: "event",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    NormalPrice = table.Column<decimal>(type: "TEXT", nullable: false),
                    AmipaPrice = table.Column<decimal>(type: "TEXT", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PublishDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UnpublishDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsAmipa = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "group",
                schema: "people",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ParentId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_group", x => x.Id);
                    table.ForeignKey(
                        name: "FK_group_group_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "people",
                        principalTable: "group",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "order",
                schema: "order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "person",
                schema: "people",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DocumentId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Surname1 = table.Column<string>(type: "TEXT", nullable: false),
                    Surname2 = table.Column<string>(type: "TEXT", nullable: true),
                    ContactPhone = table.Column<string>(type: "TEXT", maxLength: 15, nullable: true),
                    ContactMail = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_person", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                schema: "authentication",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    HashedPassword = table.Column<string>(type: "TEXT", nullable: false),
                    Firstname = table.Column<string>(type: "TEXT", nullable: false),
                    Lastname = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "item",
                schema: "order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrderId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_item", x => x.Id);
                    table.ForeignKey(
                        name: "FK_item_order_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "order",
                        principalTable: "order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "person_group_course",
                schema: "people",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PersonId = table.Column<long>(type: "INTEGER", nullable: false),
                    CourseId = table.Column<long>(type: "INTEGER", nullable: false),
                    GroupId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_person_group_course", x => x.Id);
                    table.ForeignKey(
                        name: "FK_person_group_course_course_CourseId",
                        column: x => x.CourseId,
                        principalSchema: "people",
                        principalTable: "course",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_person_group_course_group_GroupId",
                        column: x => x.GroupId,
                        principalSchema: "people",
                        principalTable: "group",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_person_group_course_person_PersonId",
                        column: x => x.PersonId,
                        principalSchema: "people",
                        principalTable: "person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "student",
                schema: "people",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AcademicRecordNumber = table.Column<long>(type: "INTEGER", nullable: false),
                    SubjectsInfo = table.Column<string>(type: "TEXT", nullable: true),
                    Amipa = table.Column<bool>(type: "INTEGER", nullable: false),
                    PreEnrollment = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student", x => x.Id);
                    table.ForeignKey(
                        name: "FK_student_person_Id",
                        column: x => x.Id,
                        principalSchema: "people",
                        principalTable: "person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_claim",
                schema: "authentication",
                columns: table => new
                {
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_claim", x => new { x.Type, x.UserId });
                    table.ForeignKey(
                        name: "FK_user_claim_user_UserId",
                        column: x => x.UserId,
                        principalSchema: "authentication",
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "event_person",
                schema: "event",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Paid = table.Column<bool>(type: "INTEGER", nullable: false),
                    PersonId = table.Column<long>(type: "INTEGER", nullable: false),
                    EventId = table.Column<long>(type: "INTEGER", nullable: false),
                    ItemId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event_person", x => x.Id);
                    table.ForeignKey(
                        name: "FK_event_person_event_EventId",
                        column: x => x.EventId,
                        principalSchema: "event",
                        principalTable: "event",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_event_person_item_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "order",
                        principalTable: "item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_event_person_person_PersonId",
                        column: x => x.PersonId,
                        principalSchema: "people",
                        principalTable: "person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_course_Active",
                schema: "people",
                table: "course",
                column: "Active");

            migrationBuilder.CreateIndex(
                name: "IX_course_Name",
                schema: "people",
                table: "course",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_course_StartDate",
                schema: "people",
                table: "course",
                column: "StartDate",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_event_Code",
                schema: "event",
                table: "event",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_event_CreationDate",
                schema: "event",
                table: "event",
                column: "CreationDate",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_event_PublishDate",
                schema: "event",
                table: "event",
                column: "PublishDate",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_event_UnpublishDate",
                schema: "event",
                table: "event",
                column: "UnpublishDate",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_event_person_EventId",
                schema: "event",
                table: "event_person",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_event_person_ItemId",
                schema: "event",
                table: "event_person",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_event_person_PersonId_EventId_ItemId",
                schema: "event",
                table: "event_person",
                columns: new[] { "PersonId", "EventId", "ItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_group_Name",
                schema: "people",
                table: "group",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_group_ParentId",
                schema: "people",
                table: "group",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_item_OrderId",
                schema: "order",
                table: "item",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_order_Created",
                schema: "order",
                table: "order",
                column: "Created",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_person_DocumentId",
                schema: "people",
                table: "person",
                column: "DocumentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_person_Name",
                schema: "people",
                table: "person",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_person_group_course_CourseId",
                schema: "people",
                table: "person_group_course",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_person_group_course_GroupId",
                schema: "people",
                table: "person_group_course",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_person_group_course_PersonId_GroupId_CourseId",
                schema: "people",
                table: "person_group_course",
                columns: new[] { "PersonId", "GroupId", "CourseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_student_AcademicRecordNumber",
                schema: "people",
                table: "student",
                column: "AcademicRecordNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_Username",
                schema: "authentication",
                table: "user",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_claim_UserId",
                schema: "authentication",
                table: "user_claim",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "event_person",
                schema: "event");

            migrationBuilder.DropTable(
                name: "person_group_course",
                schema: "people");

            migrationBuilder.DropTable(
                name: "student",
                schema: "people");

            migrationBuilder.DropTable(
                name: "user_claim",
                schema: "authentication");

            migrationBuilder.DropTable(
                name: "event",
                schema: "event");

            migrationBuilder.DropTable(
                name: "item",
                schema: "order");

            migrationBuilder.DropTable(
                name: "course",
                schema: "people");

            migrationBuilder.DropTable(
                name: "group",
                schema: "people");

            migrationBuilder.DropTable(
                name: "person",
                schema: "people");

            migrationBuilder.DropTable(
                name: "user",
                schema: "authentication");

            migrationBuilder.DropTable(
                name: "order",
                schema: "order");
        }
    }
}
