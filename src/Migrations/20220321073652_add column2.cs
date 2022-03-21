using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MjApp.Migrations
{
    public partial class addcolumn2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ItemName",
                table: "Items",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ItemCode",
                table: "Items",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CostPrice",
                table: "Items",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "SupplierItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ItemCode = table.Column<string>(type: "TEXT", nullable: false),
                    PackageQty = table.Column<decimal>(type: "TEXT", nullable: false),
                    CostPrice = table.Column<decimal>(type: "TEXT", nullable: false),
                    ItemModel = table.Column<string>(type: "TEXT", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Creator = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SupplierName = table.Column<string>(type: "TEXT", nullable: true),
                    SupplierCode = table.Column<string>(type: "TEXT", nullable: true),
                    SupplierMobile = table.Column<string>(type: "TEXT", nullable: true),
                    SupplierAddress = table.Column<string>(type: "TEXT", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Creator = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupplierItems");

            migrationBuilder.DropTable(
                name: "Suppliers");

            migrationBuilder.DropColumn(
                name: "CostPrice",
                table: "Items");

            migrationBuilder.AlterColumn<string>(
                name: "ItemName",
                table: "Items",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "ItemCode",
                table: "Items",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");
        }
    }
}
