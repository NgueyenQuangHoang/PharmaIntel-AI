using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmaIntel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RestrictCriticalUserFks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_diagnostic_sessions_users_user_id",
                table: "diagnostic_sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_health_metrics_users_user_id",
                table: "health_metrics");

            migrationBuilder.DropForeignKey(
                name: "FK_orders_users_user_id",
                table: "orders");

            migrationBuilder.DropForeignKey(
                name: "FK_payment_methods_users_user_id",
                table: "payment_methods");

            migrationBuilder.DropForeignKey(
                name: "FK_pharmacist_chat_sessions_users_user_id",
                table: "pharmacist_chat_sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_prescriptions_users_user_id",
                table: "prescriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_user_consents_users_user_id",
                table: "user_consents");

            migrationBuilder.AddForeignKey(
                name: "FK_diagnostic_sessions_users_user_id",
                table: "diagnostic_sessions",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_health_metrics_users_user_id",
                table: "health_metrics",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_orders_users_user_id",
                table: "orders",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_payment_methods_users_user_id",
                table: "payment_methods",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_pharmacist_chat_sessions_users_user_id",
                table: "pharmacist_chat_sessions",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_prescriptions_users_user_id",
                table: "prescriptions",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_user_consents_users_user_id",
                table: "user_consents",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_diagnostic_sessions_users_user_id",
                table: "diagnostic_sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_health_metrics_users_user_id",
                table: "health_metrics");

            migrationBuilder.DropForeignKey(
                name: "FK_orders_users_user_id",
                table: "orders");

            migrationBuilder.DropForeignKey(
                name: "FK_payment_methods_users_user_id",
                table: "payment_methods");

            migrationBuilder.DropForeignKey(
                name: "FK_pharmacist_chat_sessions_users_user_id",
                table: "pharmacist_chat_sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_prescriptions_users_user_id",
                table: "prescriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_user_consents_users_user_id",
                table: "user_consents");

            migrationBuilder.AddForeignKey(
                name: "FK_diagnostic_sessions_users_user_id",
                table: "diagnostic_sessions",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_health_metrics_users_user_id",
                table: "health_metrics",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_orders_users_user_id",
                table: "orders",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_payment_methods_users_user_id",
                table: "payment_methods",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_pharmacist_chat_sessions_users_user_id",
                table: "pharmacist_chat_sessions",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_prescriptions_users_user_id",
                table: "prescriptions",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_consents_users_user_id",
                table: "user_consents",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
