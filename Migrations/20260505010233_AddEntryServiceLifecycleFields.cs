using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APISales.Migrations
{
    /// <inheritdoc />
    public partial class AddEntryServiceLifecycleFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CanceledAt",
                table: "SaleEntryItemServices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveredAt",
                table: "SaleEntryItemServices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeliveredByEmployeeId",
                table: "SaleEntryItemServices",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveredToName",
                table: "SaleEntryItemServices",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryNote",
                table: "SaleEntryItemServices",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReadyAt",
                table: "SaleEntryItemServices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReceivedAt",
                table: "SaleEntryItemServices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAt",
                table: "SaleEntryItemServices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SaleEntryItemServices_DeliveredByEmployeeId",
                table: "SaleEntryItemServices",
                column: "DeliveredByEmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleEntryItemServices_Employees_DeliveredByEmployeeId",
                table: "SaleEntryItemServices",
                column: "DeliveredByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaleEntryItemServices_Employees_DeliveredByEmployeeId",
                table: "SaleEntryItemServices");

            migrationBuilder.DropIndex(
                name: "IX_SaleEntryItemServices_DeliveredByEmployeeId",
                table: "SaleEntryItemServices");

            migrationBuilder.DropColumn(
                name: "CanceledAt",
                table: "SaleEntryItemServices");

            migrationBuilder.DropColumn(
                name: "DeliveredAt",
                table: "SaleEntryItemServices");

            migrationBuilder.DropColumn(
                name: "DeliveredByEmployeeId",
                table: "SaleEntryItemServices");

            migrationBuilder.DropColumn(
                name: "DeliveredToName",
                table: "SaleEntryItemServices");

            migrationBuilder.DropColumn(
                name: "DeliveryNote",
                table: "SaleEntryItemServices");

            migrationBuilder.DropColumn(
                name: "ReadyAt",
                table: "SaleEntryItemServices");

            migrationBuilder.DropColumn(
                name: "ReceivedAt",
                table: "SaleEntryItemServices");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                table: "SaleEntryItemServices");
        }
    }
}
