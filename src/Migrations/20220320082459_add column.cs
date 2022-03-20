using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MjApp.Migrations
{
    public partial class addcolumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "Items");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Items",
                newName: "PicPath");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateTime",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Creator",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateTime",
                table: "Items",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Creator",
                table: "Items",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ItemCate",
                table: "Items",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ItemCode",
                table: "Items",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ItemModel",
                table: "Items",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ItemName",
                table: "Items",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ItemStyle",
                table: "Items",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PackageQty",
                table: "Items",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Items",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Volume",
                table: "Items",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateTime",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Creator",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreateTime",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Creator",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ItemCate",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ItemCode",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ItemModel",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ItemName",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ItemStyle",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "PackageQty",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Volume",
                table: "Items");

            migrationBuilder.RenameColumn(
                name: "PicPath",
                table: "Items",
                newName: "Title");

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "Items",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
