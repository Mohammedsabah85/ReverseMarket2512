using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReverseMarket.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiLanguageSiteSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AboutUsEn",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AboutUsKu",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CopyrightInfoEn",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CopyrightInfoKu",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IntellectualProperty",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IntellectualPropertyEn",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IntellectualPropertyKu",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrivacyPolicyEn",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrivacyPolicyKu",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TermsOfUseEn",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TermsOfUseKu",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AboutUsEn",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "AboutUsKu",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "CopyrightInfoEn",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "CopyrightInfoKu",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "IntellectualProperty",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "IntellectualPropertyEn",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "IntellectualPropertyKu",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "PrivacyPolicyEn",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "PrivacyPolicyKu",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "TermsOfUseEn",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "TermsOfUseKu",
                table: "SiteSettings");
        }
    }
}
