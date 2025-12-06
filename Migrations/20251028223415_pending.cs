using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReverseMarket.Migrations
{
    /// <inheritdoc />
    public partial class pending : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasPendingUrlChanges",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PendingWebsiteUrl1",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PendingWebsiteUrl2",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PendingWebsiteUrl3",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UrlsLastApprovedAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-id-12345",
                columns: new[] { "HasPendingUrlChanges", "PendingWebsiteUrl1", "PendingWebsiteUrl2", "PendingWebsiteUrl3", "UrlsLastApprovedAt" },
                values: new object[] { false, null, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasPendingUrlChanges",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PendingWebsiteUrl1",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PendingWebsiteUrl2",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PendingWebsiteUrl3",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UrlsLastApprovedAt",
                table: "Users");
        }
    }
}
