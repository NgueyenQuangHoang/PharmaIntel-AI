using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmaIntel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixDataIntegrityFKs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_pharmacist_chat_messages_sender_pharmacist_id",
                table: "pharmacist_chat_messages",
                column: "sender_pharmacist_id");

            migrationBuilder.CreateIndex(
                name: "IX_pharmacist_chat_messages_sender_user_id",
                table: "pharmacist_chat_messages",
                column: "sender_user_id");

            migrationBuilder.AddCheckConstraint(
                name: "CK_pharmacist_chat_messages_sender_consistency",
                table: "pharmacist_chat_messages",
                sql: "\r\n                ([sender_type] = 'user'       AND [sender_user_id] IS NOT NULL AND [sender_pharmacist_id] IS NULL)\r\n             OR ([sender_type] = 'pharmacist' AND [sender_user_id] IS NULL     AND [sender_pharmacist_id] IS NOT NULL)\r\n             OR ([sender_type] = 'system'     AND [sender_user_id] IS NULL     AND [sender_pharmacist_id] IS NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_prescription_item_id",
                table: "order_items",
                column: "prescription_item_id");

            migrationBuilder.AddForeignKey(
                name: "FK_order_items_prescription_items_prescription_item_id",
                table: "order_items",
                column: "prescription_item_id",
                principalTable: "prescription_items",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_pharmacist_chat_messages_pharmacists_sender_pharmacist_id",
                table: "pharmacist_chat_messages",
                column: "sender_pharmacist_id",
                principalTable: "pharmacists",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_pharmacist_chat_messages_users_sender_user_id",
                table: "pharmacist_chat_messages",
                column: "sender_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_order_items_prescription_items_prescription_item_id",
                table: "order_items");

            migrationBuilder.DropForeignKey(
                name: "FK_pharmacist_chat_messages_pharmacists_sender_pharmacist_id",
                table: "pharmacist_chat_messages");

            migrationBuilder.DropForeignKey(
                name: "FK_pharmacist_chat_messages_users_sender_user_id",
                table: "pharmacist_chat_messages");

            migrationBuilder.DropIndex(
                name: "IX_pharmacist_chat_messages_sender_pharmacist_id",
                table: "pharmacist_chat_messages");

            migrationBuilder.DropIndex(
                name: "IX_pharmacist_chat_messages_sender_user_id",
                table: "pharmacist_chat_messages");

            migrationBuilder.DropCheckConstraint(
                name: "CK_pharmacist_chat_messages_sender_consistency",
                table: "pharmacist_chat_messages");

            migrationBuilder.DropIndex(
                name: "IX_order_items_prescription_item_id",
                table: "order_items");
        }
    }
}
