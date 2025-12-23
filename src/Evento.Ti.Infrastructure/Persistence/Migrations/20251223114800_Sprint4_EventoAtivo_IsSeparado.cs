using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Evento.Ti.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Sprint4_EventoAtivo_IsSeparado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSeparado",
                table: "eventos_ativos",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSeparado",
                table: "eventos_ativos");
        }
    }
}
