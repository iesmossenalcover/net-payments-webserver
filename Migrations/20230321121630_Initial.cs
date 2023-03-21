using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

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
                name: "main");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:CollationDefinition:no_accent", "und-u-ks-level1-kc-true,und-u-ks-level1-kc-true,icu,False");

            migrationBuilder.CreateTable(
                name: "course",
                schema: "main",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StartDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_course", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "group",
                schema: "main",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ParentId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_group", x => x.Id);
                    table.ForeignKey(
                        name: "FK_group_group_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "main",
                        principalTable: "group",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "person",
                schema: "main",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocumentId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Surname1 = table.Column<string>(type: "text", nullable: false),
                    Surname2 = table.Column<string>(type: "text", nullable: true),
                    ContactPhone = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    ContactMail = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AcademicRecordNumber = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_person", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                schema: "main",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    HashedPassword = table.Column<string>(type: "text", nullable: false),
                    Firstname = table.Column<string>(type: "text", nullable: false),
                    Lastname = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "event",
                schema: "main",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    AmipaPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    Enrollment = table.Column<bool>(type: "boolean", nullable: false),
                    Amipa = table.Column<bool>(type: "boolean", nullable: false),
                    CreationDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PublishDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UnpublishDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CourseId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event", x => x.Id);
                    table.ForeignKey(
                        name: "FK_event_course_CourseId",
                        column: x => x.CourseId,
                        principalSchema: "main",
                        principalTable: "course",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order",
                schema: "main",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PersonId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order", x => x.Id);
                    table.ForeignKey(
                        name: "FK_order_person_PersonId",
                        column: x => x.PersonId,
                        principalSchema: "main",
                        principalTable: "person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_claim",
                schema: "main",
                columns: table => new
                {
                    Type = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_claim", x => new { x.Type, x.UserId });
                    table.ForeignKey(
                        name: "FK_user_claim_user_UserId",
                        column: x => x.UserId,
                        principalSchema: "main",
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "person_group_course",
                schema: "main",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PersonId = table.Column<long>(type: "bigint", nullable: false),
                    CourseId = table.Column<long>(type: "bigint", nullable: false),
                    GroupId = table.Column<long>(type: "bigint", nullable: false),
                    Amipa = table.Column<bool>(type: "boolean", nullable: false),
                    Enrolled = table.Column<bool>(type: "boolean", nullable: false),
                    EnrollmentEventId = table.Column<long>(type: "bigint", nullable: true),
                    SubjectsInfo = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_person_group_course", x => x.Id);
                    table.ForeignKey(
                        name: "FK_person_group_course_course_CourseId",
                        column: x => x.CourseId,
                        principalSchema: "main",
                        principalTable: "course",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_person_group_course_event_EnrollmentEventId",
                        column: x => x.EnrollmentEventId,
                        principalSchema: "main",
                        principalTable: "event",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_person_group_course_group_GroupId",
                        column: x => x.GroupId,
                        principalSchema: "main",
                        principalTable: "group",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_person_group_course_person_PersonId",
                        column: x => x.PersonId,
                        principalSchema: "main",
                        principalTable: "person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "event_person",
                schema: "main",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Paid = table.Column<bool>(type: "boolean", nullable: false),
                    PersonId = table.Column<long>(type: "bigint", nullable: false),
                    EventId = table.Column<long>(type: "bigint", nullable: false),
                    OrderId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event_person", x => x.Id);
                    table.ForeignKey(
                        name: "FK_event_person_event_EventId",
                        column: x => x.EventId,
                        principalSchema: "main",
                        principalTable: "event",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_event_person_order_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "main",
                        principalTable: "order",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_event_person_person_PersonId",
                        column: x => x.PersonId,
                        principalSchema: "main",
                        principalTable: "person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_course_Active",
                schema: "main",
                table: "course",
                column: "Active");

            migrationBuilder.CreateIndex(
                name: "IX_course_Name",
                schema: "main",
                table: "course",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_course_StartDate",
                schema: "main",
                table: "course",
                column: "StartDate",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_event_Code",
                schema: "main",
                table: "event",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_event_CourseId",
                schema: "main",
                table: "event",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_event_CreationDate",
                schema: "main",
                table: "event",
                column: "CreationDate",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_event_PublishDate",
                schema: "main",
                table: "event",
                column: "PublishDate",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_event_UnpublishDate",
                schema: "main",
                table: "event",
                column: "UnpublishDate",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_event_person_EventId",
                schema: "main",
                table: "event_person",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_event_person_OrderId",
                schema: "main",
                table: "event_person",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_event_person_PersonId_EventId_OrderId",
                schema: "main",
                table: "event_person",
                columns: new[] { "PersonId", "EventId", "OrderId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_group_Name",
                schema: "main",
                table: "group",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_group_ParentId",
                schema: "main",
                table: "group",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_order_Created",
                schema: "main",
                table: "order",
                column: "Created",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_order_PersonId",
                schema: "main",
                table: "order",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_person_AcademicRecordNumber",
                schema: "main",
                table: "person",
                column: "AcademicRecordNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_person_DocumentId",
                schema: "main",
                table: "person",
                column: "DocumentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_person_Name",
                schema: "main",
                table: "person",
                column: "Name")
                .Annotation("Relational:Collation", new[] { "no_accent" });

            migrationBuilder.CreateIndex(
                name: "IX_person_Surname1",
                schema: "main",
                table: "person",
                column: "Surname1")
                .Annotation("Relational:Collation", new[] { "no_accent" });

            migrationBuilder.CreateIndex(
                name: "IX_person_Surname2",
                schema: "main",
                table: "person",
                column: "Surname2")
                .Annotation("Relational:Collation", new[] { "no_accent" });

            migrationBuilder.CreateIndex(
                name: "IX_person_group_course_CourseId",
                schema: "main",
                table: "person_group_course",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_person_group_course_EnrollmentEventId",
                schema: "main",
                table: "person_group_course",
                column: "EnrollmentEventId");

            migrationBuilder.CreateIndex(
                name: "IX_person_group_course_GroupId",
                schema: "main",
                table: "person_group_course",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_person_group_course_PersonId_GroupId_CourseId",
                schema: "main",
                table: "person_group_course",
                columns: new[] { "PersonId", "GroupId", "CourseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_Username",
                schema: "main",
                table: "user",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_claim_UserId",
                schema: "main",
                table: "user_claim",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "event_person",
                schema: "main");

            migrationBuilder.DropTable(
                name: "person_group_course",
                schema: "main");

            migrationBuilder.DropTable(
                name: "user_claim",
                schema: "main");

            migrationBuilder.DropTable(
                name: "order",
                schema: "main");

            migrationBuilder.DropTable(
                name: "event",
                schema: "main");

            migrationBuilder.DropTable(
                name: "group",
                schema: "main");

            migrationBuilder.DropTable(
                name: "user",
                schema: "main");

            migrationBuilder.DropTable(
                name: "person",
                schema: "main");

            migrationBuilder.DropTable(
                name: "course",
                schema: "main");
        }
    }
}
