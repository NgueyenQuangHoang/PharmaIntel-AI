using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmaIntel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "role",
                table: "users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "user");

            migrationBuilder.AddCheckConstraint(
                name: "CK_users_role",
                table: "users",
                sql: "[role] IN ('user','admin')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_users_role",
                table: "users");

            migrationBuilder.DropColumn(
                name: "role",
                table: "users");
        }
    }
}
