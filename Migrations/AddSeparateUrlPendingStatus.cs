using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReverseMarket.Migrations
{
    /// <inheritdoc />
    public partial class AddSeparateUrlPendingStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // إضافة حقول حالة كل رابط معلق
            migrationBuilder.AddColumn<string>(
                name: "PendingUrl1Status",
                table: "AspNetUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PendingUrl2Status",
                table: "AspNetUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PendingUrl3Status",
                table: "AspNetUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            // إضافة حقول تاريخ تقديم كل رابط
            migrationBuilder.AddColumn<DateTime>(
                name: "PendingUrl1SubmittedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PendingUrl2SubmittedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PendingUrl3SubmittedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PendingUrl1Status",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PendingUrl2Status",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PendingUrl3Status",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PendingUrl1SubmittedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PendingUrl2SubmittedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PendingUrl3SubmittedAt",
                table: "AspNetUsers");
        }
    }
}