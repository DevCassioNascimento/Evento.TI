using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Evento.Ti.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEventPresence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "eventos_presencas",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eventos_presencas", x => new { x.EventId, x.UserId });
                    table.ForeignKey(
                        name: "FK_eventos_presencas_eventos_EventId",
                        column: x => x.EventId,
                        principalTable: "eventos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_eventos_presencas_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_eventos_presencas_EventId",
                table: "eventos_presencas",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_eventos_presencas_UserId",
                table: "eventos_presencas",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "eventos_presencas");
        }
    }
}
