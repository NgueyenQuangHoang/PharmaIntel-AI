using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmaIntel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPharmacistRoleAndUserLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_users_role",
                table: "users");

            migrationBuilder.AddColumn<long>(
                name: "user_id",
                table: "pharmacists",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_users_role",
                table: "users",
                sql: "[role] IN ('user','admin','pharmacist')");

            migrationBuilder.CreateIndex(
                name: "UX_pharmacists_user_id",
                table: "pharmacists",
                column: "user_id",
                unique: true,
                filter: "[user_id] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_pharmacists_users_user_id",
                table: "pharmacists",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_pharmacists_users_user_id",
                table: "pharmacists");

            migrationBuilder.DropCheckConstraint(
                name: "CK_users_role",
                table: "users");

            migrationBuilder.DropIndex(
                name: "UX_pharmacists_user_id",
                table: "pharmacists");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "pharmacists");

            migrationBuilder.AddCheckConstraint(
                name: "CK_users_role",
                table: "users",
                sql: "[role] IN ('user','admin')");
        }
    }
}
