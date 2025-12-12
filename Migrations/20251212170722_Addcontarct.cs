using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReverseMarket.Migrations
{
    /// <inheritdoc />
    public partial class Addcontarct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowEmail",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowInAppChat",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowPhoneCall",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowSMS",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowWhatsApp",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AlternativeWhatsApp",
                table: "Users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactNotes",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredContactTime",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-id-12345",
                columns: new[] { "AllowEmail", "AllowInAppChat", "AllowPhoneCall", "AllowSMS", "AllowWhatsApp", "AlternativeWhatsApp", "ContactNotes", "PreferredContactTime" },
                values: new object[] { true, true, true, false, true, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowEmail",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AllowInAppChat",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AllowPhoneCall",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AllowSMS",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AllowWhatsApp",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AlternativeWhatsApp",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ContactNotes",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PreferredContactTime",
                table: "Users");
        }
    }
}
