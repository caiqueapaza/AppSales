using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APISales.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceTypeToServiceItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ServiceType",
                table: "ServiceItens",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "Geral");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServiceType",
                table: "ServiceItens");
        }
    }
}
