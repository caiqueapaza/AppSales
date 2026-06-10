using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APISales.Migrations
{
    /// <inheritdoc />
    public partial class AddMeasurementUnitToSaleEntryItemService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MeasurementUnit",
                table: "SaleEntryItemServices",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MeasurementUnit",
                table: "SaleEntryItemServices");
        }
    }
}
