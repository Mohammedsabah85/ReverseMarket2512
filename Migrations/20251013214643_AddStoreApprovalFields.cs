using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReverseMarket.Migrations
{
    /// <inheritdoc />
    public partial class AddStoreApprovalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsStoreApproved",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "StoreApprovedAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StoreApprovedBy",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-id-12345",
                columns: new[] { "IsStoreApproved", "StoreApprovedAt", "StoreApprovedBy" },
                values: new object[] { false, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsStoreApproved",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "StoreApprovedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "StoreApprovedBy",
                table: "Users");
        }
    }
}
