using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace APISales.Migrations
{
    /// <inheritdoc />
    public partial class AddEntryItemsAndServices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SaleEntryItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SaleId = table.Column<int>(type: "integer", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ConditionNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleEntryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaleEntryItems_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SaleEntryItems_Sales_SaleId",
                        column: x => x.SaleId,
                        principalTable: "Sales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SaleEntryItemServices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SaleEntryItemId = table.Column<int>(type: "integer", nullable: false),
                    ServiceItemId = table.Column<int>(type: "integer", nullable: false),
                    ExecutorEmployeeId = table.Column<int>(type: "integer", nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    RepairDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ItemStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleEntryItemServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaleEntryItemServices_Employees_ExecutorEmployeeId",
                        column: x => x.ExecutorEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SaleEntryItemServices_SaleEntryItems_SaleEntryItemId",
                        column: x => x.SaleEntryItemId,
                        principalTable: "SaleEntryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SaleEntryItemServices_ServiceItens_ServiceItemId",
                        column: x => x.ServiceItemId,
                        principalTable: "ServiceItens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SaleEntryItems_CategoryId",
                table: "SaleEntryItems",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleEntryItems_SaleId",
                table: "SaleEntryItems",
                column: "SaleId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleEntryItemServices_ExecutorEmployeeId",
                table: "SaleEntryItemServices",
                column: "ExecutorEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleEntryItemServices_SaleEntryItemId",
                table: "SaleEntryItemServices",
                column: "SaleEntryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleEntryItemServices_ServiceItemId",
                table: "SaleEntryItemServices",
                column: "ServiceItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SaleEntryItemServices");

            migrationBuilder.DropTable(
                name: "SaleEntryItems");
        }
    }
}
