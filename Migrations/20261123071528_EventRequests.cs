using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace netpaymentswebserver.Migrations
{
    /// <inheritdoc />
    public partial class EventRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "event_request",
                schema: "main",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RequesterUserId = table.Column<long>(type: "bigint", nullable: false),
                    CourseId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    StartDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    PaymentDeadline = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EventId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event_request", x => x.Id);
                    table.ForeignKey(
                        name: "FK_event_request_course_CourseId",
                        column: x => x.CourseId,
                        principalSchema: "main",
                        principalTable: "course",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_event_request_event_EventId",
                        column: x => x.EventId,
                        principalSchema: "main",
                        principalTable: "event",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_event_request_user_RequesterUserId",
                        column: x => x.RequesterUserId,
                        principalSchema: "main",
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "event_request_recipient",
                schema: "main",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventRequestId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    ReferenceId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event_request_recipient", x => x.Id);
                    table.ForeignKey(
                        name: "FK_event_request_recipient_event_request_EventRequestId",
                        column: x => x.EventRequestId,
                        principalSchema: "main",
                        principalTable: "event_request",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "event_request_transition",
                schema: "main",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventRequestId = table.Column<long>(type: "bigint", nullable: false),
                    From = table.Column<int>(type: "integer", nullable: true),
                    To = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    ActorUserId = table.Column<long>(type: "bigint", nullable: true),
                    Automatic = table.Column<bool>(type: "boolean", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    Date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event_request_transition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_event_request_transition_event_request_EventRequestId",
                        column: x => x.EventRequestId,
                        principalSchema: "main",
                        principalTable: "event_request",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_event_request_transition_user_ActorUserId",
                        column: x => x.ActorUserId,
                        principalSchema: "main",
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_event_request_CourseId",
                schema: "main",
                table: "event_request",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_event_request_EventId",
                schema: "main",
                table: "event_request",
                column: "EventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_event_request_RequesterUserId",
                schema: "main",
                table: "event_request",
                column: "RequesterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_event_request_Status",
                schema: "main",
                table: "event_request",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_event_request_recipient_EventRequestId",
                schema: "main",
                table: "event_request_recipient",
                column: "EventRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_event_request_transition_ActorUserId",
                schema: "main",
                table: "event_request_transition",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_event_request_transition_EventRequestId",
                schema: "main",
                table: "event_request_transition",
                column: "EventRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "event_request_recipient",
                schema: "main");

            migrationBuilder.DropTable(
                name: "event_request_transition",
                schema: "main");

            migrationBuilder.DropTable(
                name: "event_request",
                schema: "main");
        }
    }
}
