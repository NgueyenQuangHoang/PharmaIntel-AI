using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmaIntel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BackfillReminderLegacyData : Migration
    {
        // Don dep du lieu cu cua medication_reminders:
        //  - start_date = '0001-01-01' la default cua migration AddReminderDateRange ap
        //    cho row da ton tai khi them cot. Hien thi tren UI thanh "1/1/1" lung tung.
        //  - prescription_item_id = 0 la legacy data (manual SQL hoac code cu) - validator
        //    backend chan voi loi "PrescriptionItemId khong hop le" khi user thao tac.
        // Khong dung Down() vi du lieu da bi mat (khong biet gia tri goc).

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Lay date cua updated_at lam start_date gan dung nhat. Neu updated_at cung
            // bi lung (rat hiem), fallback ve hom nay.
            migrationBuilder.Sql(@"
                UPDATE medication_reminders
                SET start_date = COALESCE(
                    CAST(NULLIF(CAST(updated_at AS DATE), '0001-01-01') AS DATE),
                    CAST(SYSUTCDATETIME() AS DATE)
                )
                WHERE start_date = '0001-01-01';
            ");

            // prescription_item_id = 0 -> NULL (reminder thanh standalone, khong link
            // toi item ao). FK constraint Restrict khong chan vi 0 khong ton tai trong
            // prescription_items.
            migrationBuilder.Sql(@"
                UPDATE medication_reminders
                SET prescription_item_id = NULL
                WHERE prescription_item_id = 0;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Khong revert duoc - du lieu cu da mat.
        }
    }
}
