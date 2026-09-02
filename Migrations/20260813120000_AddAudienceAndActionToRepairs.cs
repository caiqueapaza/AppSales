using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APISales.Migrations
{
    /// <inheritdoc />
    public partial class AddAudienceAndActionToRepairs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AudienceType",
                table: "SaleEntryItems",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Adult");

            migrationBuilder.AddColumn<string>(
                name: "ActionType",
                table: "SaleEntryItemServices",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Adjustment");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AudienceType",
                table: "SaleEntryItems");

            migrationBuilder.DropColumn(
                name: "ActionType",
                table: "SaleEntryItemServices");
        }
    }
}
