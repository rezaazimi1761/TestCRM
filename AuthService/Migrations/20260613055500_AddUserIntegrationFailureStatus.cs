using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthService.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIntegrationFailureStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IntegrationError",
                table: "Users",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IntegrationStatus",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FaultReason",
                table: "UserIntegrationSagas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FaultedAt",
                table: "UserIntegrationSagas",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IntegrationError",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IntegrationStatus",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FaultReason",
                table: "UserIntegrationSagas");

            migrationBuilder.DropColumn(
                name: "FaultedAt",
                table: "UserIntegrationSagas");
        }
    }
}
