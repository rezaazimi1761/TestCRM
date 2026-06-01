using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthService.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceInstances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ServiceInstanceId",
                table: "Tenants",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ServiceInstances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ApiUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceInstances", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_ServiceInstanceId",
                table: "Tenants",
                column: "ServiceInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceInstances_Name",
                table: "ServiceInstances",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tenants_ServiceInstances_ServiceInstanceId",
                table: "Tenants",
                column: "ServiceInstanceId",
                principalTable: "ServiceInstances",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tenants_ServiceInstances_ServiceInstanceId",
                table: "Tenants");

            migrationBuilder.DropTable(
                name: "ServiceInstances");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_ServiceInstanceId",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "ServiceInstanceId",
                table: "Tenants");
        }
    }
}
