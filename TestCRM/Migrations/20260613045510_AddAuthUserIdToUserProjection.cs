using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestCRM.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthUserIdToUserProjection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AuthUserId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_AuthUserId_TenantId",
                table: "Users",
                columns: new[] { "AuthUserId", "TenantId" },
                unique: true,
                filter: "[AuthUserId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_AuthUserId_TenantId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AuthUserId",
                table: "Users");
        }
    }
}
