using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReverseMarket.Migrations
{
    /// <inheritdoc />
    public partial class intcrateat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IntellectualProperty_AR",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "IntellectualProperty_EN",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "IntellectualProperty_KU",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "PrivacyPolicy_AR",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "PrivacyPolicy_EN",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "PrivacyPolicy_KU",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "TermsOfUse_AR",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "TermsOfUse_EN",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "TermsOfUse_KU",
                table: "SiteSettings");

            migrationBuilder.AlterColumn<string>(
                name: "CopyrightInfoKu",
                table: "SiteSettings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CopyrightInfoEn",
                table: "SiteSettings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AboutUsKu",
                table: "SiteSettings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AboutUsEn",
                table: "SiteSettings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CopyrightInfoKu",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CopyrightInfoEn",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AboutUsKu",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AboutUsEn",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IntellectualProperty_AR",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IntellectualProperty_EN",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IntellectualProperty_KU",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrivacyPolicy_AR",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrivacyPolicy_EN",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrivacyPolicy_KU",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TermsOfUse_AR",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TermsOfUse_EN",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TermsOfUse_KU",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
