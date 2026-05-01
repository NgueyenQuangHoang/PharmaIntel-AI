using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmaIntel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    parent_id = table.Column<long>(type: "bigint", nullable: true),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    icon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    display_order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.id);
                    table.ForeignKey(
                        name: "FK_categories_categories_parent_id",
                        column: x => x.parent_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "doctors",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    full_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    license_number = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    specialization = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    hospital = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    avatar_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_doctors", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pharmacists",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    full_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    license_number = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    specialization = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    avatar_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    is_online = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pharmacists", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "symptoms",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    group_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    display_order = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_symptoms", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    full_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    avatar_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    auth_provider = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "local"),
                    auth_provider_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    is_terms_accepted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.CheckConstraint("CK_users_auth_provider", "[auth_provider] IN ('local','google','apple')");
                });

            migrationBuilder.CreateTable(
                name: "medications",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    sku = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    generic_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    manufacturer = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    registration_number = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dosage = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    packaging = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    price = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    discount_percent = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 0m),
                    category_id = table.Column<long>(type: "bigint", nullable: false),
                    usage_instructions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    benefits = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    active_ingredients = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    contraindications = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    side_effects = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    storage_instructions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    image_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    is_featured = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    is_best_seller = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    is_prescription_required = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    stock_quantity = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medications", x => x.id);
                    table.CheckConstraint("CK_medications_discount", "[discount_percent] >= 0 AND [discount_percent] <= 100");
                    table.CheckConstraint("CK_medications_price", "[price] >= 0");
                    table.CheckConstraint("CK_medications_stock", "[stock_quantity] >= 0");
                    table.ForeignKey(
                        name: "FK_medications_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "addresses",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    recipient_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    province = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    district = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ward = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    street_address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    is_default = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_addresses", x => x.id);
                    table.ForeignKey(
                        name: "FK_addresses_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ai_insights",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    insight_type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    generated_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_insights", x => x.id);
                    table.CheckConstraint("CK_ai_insights_type", "[insight_type] IN ('health_summary','medication','diagnostic','lifestyle','system')");
                    table.ForeignKey(
                        name: "FK_ai_insights_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    actor_user_id = table.Column<long>(type: "bigint", nullable: true),
                    action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    entity_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    entity_id = table.Column<long>(type: "bigint", nullable: true),
                    old_values = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    new_values = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_audit_logs_users_actor_user_id",
                        column: x => x.actor_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "diagnostic_sessions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "in_progress"),
                    created_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    completed_at = table.Column<DateTime>(type: "datetime2(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_diagnostic_sessions", x => x.id);
                    table.CheckConstraint("CK_diagnostic_sessions_status", "[status] IN ('in_progress','analyzing','completed','cancelled','failed')");
                    table.ForeignKey(
                        name: "FK_diagnostic_sessions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "health_metrics",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    metric_type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    value_number = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    value_number2 = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    recorded_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_health_metrics", x => x.id);
                    table.CheckConstraint("CK_health_metrics_type", "[metric_type] IN ('blood_pressure','heart_rate','temperature','weight','blood_sugar','oxygen_saturation')");
                    table.ForeignKey(
                        name: "FK_health_metrics_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    notification_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    reference_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    reference_id = table.Column<long>(type: "bigint", nullable: true),
                    is_read = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    read_at = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_notifications_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payment_methods",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    payment_type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    display_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    masked_account = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    provider_customer_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    is_default = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_methods", x => x.id);
                    table.CheckConstraint("CK_payment_methods_type", "[payment_type] IN ('cod','bank_transfer','momo','zalopay','vnpay','credit_card')");
                    table.ForeignKey(
                        name: "FK_payment_methods_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pharmacist_chat_sessions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    pharmacist_id = table.Column<long>(type: "bigint", nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "open"),
                    started_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    closed_at = table.Column<DateTime>(type: "datetime2(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pharmacist_chat_sessions", x => x.id);
                    table.CheckConstraint("CK_pharmacist_chat_sessions_status", "[status] IN ('open','waiting','closed','cancelled')");
                    table.ForeignKey(
                        name: "FK_pharmacist_chat_sessions_pharmacists_pharmacist_id",
                        column: x => x.pharmacist_id,
                        principalTable: "pharmacists",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_pharmacist_chat_sessions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prescriptions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    doctor_id = table.Column<long>(type: "bigint", nullable: true),
                    doctor_name_snapshot = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    prescribed_date = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "draft"),
                    verification_status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "not_required"),
                    created_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prescriptions", x => x.id);
                    table.CheckConstraint("CK_prescriptions_status", "[status] IN ('draft','active','completed','expired','cancelled')");
                    table.CheckConstraint("CK_prescriptions_verification", "[verification_status] IN ('not_required','pending','verified','rejected')");
                    table.ForeignKey(
                        name: "FK_prescriptions_doctors_doctor_id",
                        column: x => x.doctor_id,
                        principalTable: "doctors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_prescriptions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_consents",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    consent_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    consent_version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    accepted_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    revoked_at = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    ip_address = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_consents", x => x.id);
                    table.CheckConstraint("CK_user_consents_type", "[consent_type] IN ('terms','privacy','medical_ai_disclaimer','marketing')");
                    table.ForeignKey(
                        name: "FK_user_consents_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_settings",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    dark_mode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "system"),
                    language_code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "vi"),
                    notification_enabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    reminder_sound_enabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_settings", x => x.id);
                    table.CheckConstraint("CK_user_settings_dark_mode", "[dark_mode] IN ('light','dark','system')");
                    table.ForeignKey(
                        name: "FK_user_settings_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cart_items",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    medication_id = table.Column<long>(type: "bigint", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    added_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cart_items", x => x.id);
                    table.CheckConstraint("CK_cart_items_quantity", "[quantity] > 0");
                    table.ForeignKey(
                        name: "FK_cart_items_medications_medication_id",
                        column: x => x.medication_id,
                        principalTable: "medications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_cart_items_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "diagnostic_messages",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    session_id = table.Column<long>(type: "bigint", nullable: false),
                    sender_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sent_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_diagnostic_messages", x => x.id);
                    table.CheckConstraint("CK_diagnostic_messages_sender", "[sender_type] IN ('user','ai','system')");
                    table.ForeignKey(
                        name: "FK_diagnostic_messages_diagnostic_sessions_session_id",
                        column: x => x.session_id,
                        principalTable: "diagnostic_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "diagnostic_results",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    session_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    ai_conclusion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    confidence_score = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    risk_level = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "low"),
                    red_flags = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    requires_doctor_visit = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    model_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    model_version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    diagnosed_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_diagnostic_results", x => x.id);
                    table.CheckConstraint("CK_diagnostic_results_confidence", "[confidence_score] >= 0 AND [confidence_score] <= 100");
                    table.CheckConstraint("CK_diagnostic_results_risk", "[risk_level] IN ('low','medium','high','emergency')");
                    table.ForeignKey(
                        name: "FK_diagnostic_results_diagnostic_sessions_session_id",
                        column: x => x.session_id,
                        principalTable: "diagnostic_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_diagnostic_results_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "diagnostic_session_symptoms",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    session_id = table.Column<long>(type: "bigint", nullable: false),
                    symptom_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_diagnostic_session_symptoms", x => x.id);
                    table.ForeignKey(
                        name: "FK_diagnostic_session_symptoms_diagnostic_sessions_session_id",
                        column: x => x.session_id,
                        principalTable: "diagnostic_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_diagnostic_session_symptoms_symptoms_symptom_id",
                        column: x => x.symptom_id,
                        principalTable: "symptoms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pharmacist_chat_messages",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    session_id = table.Column<long>(type: "bigint", nullable: false),
                    sender_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    sender_user_id = table.Column<long>(type: "bigint", nullable: true),
                    sender_pharmacist_id = table.Column<long>(type: "bigint", nullable: true),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sent_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pharmacist_chat_messages", x => x.id);
                    table.CheckConstraint("CK_pharmacist_chat_messages_sender", "[sender_type] IN ('user','pharmacist','system')");
                    table.ForeignKey(
                        name: "FK_pharmacist_chat_messages_pharmacist_chat_sessions_session_id",
                        column: x => x.session_id,
                        principalTable: "pharmacist_chat_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    address_id = table.Column<long>(type: "bigint", nullable: true),
                    payment_method_id = table.Column<long>(type: "bigint", nullable: true),
                    prescription_id = table.Column<long>(type: "bigint", nullable: true),
                    order_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    subtotal = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    shipping_fee = table.Column<decimal>(type: "decimal(12,2)", nullable: false, defaultValue: 0m),
                    total = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    shipping_recipient_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    shipping_phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    shipping_full_address = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    payment_type_snapshot = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    payment_status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "unpaid"),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "pending"),
                    created_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders", x => x.id);
                    table.CheckConstraint("CK_orders_payment_status", "[payment_status] IN ('unpaid','pending','paid','failed','refunded','cod_pending')");
                    table.CheckConstraint("CK_orders_status", "[status] IN ('pending','confirmed','processing','shipping','delivered','cancelled','refunded')");
                    table.CheckConstraint("CK_orders_total", "[total] >= 0");
                    table.ForeignKey(
                        name: "FK_orders_addresses_address_id",
                        column: x => x.address_id,
                        principalTable: "addresses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_orders_payment_methods_payment_method_id",
                        column: x => x.payment_method_id,
                        principalTable: "payment_methods",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_orders_prescriptions_prescription_id",
                        column: x => x.prescription_id,
                        principalTable: "prescriptions",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_orders_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prescription_documents",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    prescription_id = table.Column<long>(type: "bigint", nullable: false),
                    file_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    verification_status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "pending"),
                    verified_by_pharmacist_id = table.Column<long>(type: "bigint", nullable: true),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prescription_documents", x => x.id);
                    table.CheckConstraint("CK_prescription_documents_verification", "[verification_status] IN ('pending','verified','rejected')");
                    table.ForeignKey(
                        name: "FK_prescription_documents_pharmacists_verified_by_pharmacist_id",
                        column: x => x.verified_by_pharmacist_id,
                        principalTable: "pharmacists",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_prescription_documents_prescriptions_prescription_id",
                        column: x => x.prescription_id,
                        principalTable: "prescriptions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prescription_items",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    prescription_id = table.Column<long>(type: "bigint", nullable: false),
                    medication_id = table.Column<long>(type: "bigint", nullable: true),
                    medication_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    dosage = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    frequency = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    duration = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prescription_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_prescription_items_medications_medication_id",
                        column: x => x.medication_id,
                        principalTable: "medications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_prescription_items_prescriptions_prescription_id",
                        column: x => x.prescription_id,
                        principalTable: "prescriptions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "diagnostic_result_medications",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    result_id = table.Column<long>(type: "bigint", nullable: false),
                    medication_id = table.Column<long>(type: "bigint", nullable: false),
                    priority = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_diagnostic_result_medications", x => x.id);
                    table.CheckConstraint("CK_diagnostic_result_medications_priority", "[priority] > 0");
                    table.ForeignKey(
                        name: "FK_diagnostic_result_medications_diagnostic_results_result_id",
                        column: x => x.result_id,
                        principalTable: "diagnostic_results",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_diagnostic_result_medications_medications_medication_id",
                        column: x => x.medication_id,
                        principalTable: "medications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "order_items",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    medication_id = table.Column<long>(type: "bigint", nullable: true),
                    prescription_item_id = table.Column<long>(type: "bigint", nullable: true),
                    medication_name_snapshot = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    unit_price = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    discount_percent = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 0m),
                    total_price = table.Column<decimal>(type: "decimal(12,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_items", x => x.id);
                    table.CheckConstraint("CK_order_items_price", "[unit_price] >= 0");
                    table.CheckConstraint("CK_order_items_quantity", "[quantity] > 0");
                    table.CheckConstraint("CK_order_items_total", "[total_price] >= 0");
                    table.ForeignKey(
                        name: "FK_order_items_medications_medication_id",
                        column: x => x.medication_id,
                        principalTable: "medications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_order_items_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payment_transactions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    payment_method_id = table.Column<long>(type: "bigint", nullable: true),
                    provider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    provider_transaction_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    amount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "initiated"),
                    created_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_transactions", x => x.id);
                    table.CheckConstraint("CK_payment_transactions_amount", "[amount] >= 0");
                    table.CheckConstraint("CK_payment_transactions_status", "[status] IN ('initiated','pending','success','failed','cancelled','refunded')");
                    table.ForeignKey(
                        name: "FK_payment_transactions_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_payment_transactions_payment_methods_payment_method_id",
                        column: x => x.payment_method_id,
                        principalTable: "payment_methods",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "medication_reminders",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    prescription_item_id = table.Column<long>(type: "bigint", nullable: true),
                    medication_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    frequency_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "daily"),
                    reminder_time = table.Column<TimeOnly>(type: "time(0)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "active"),
                    created_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medication_reminders", x => x.id);
                    table.CheckConstraint("CK_medication_reminders_frequency", "[frequency_type] IN ('once','daily','weekly','custom')");
                    table.CheckConstraint("CK_medication_reminders_status", "[status] IN ('active','paused','completed','cancelled')");
                    table.ForeignKey(
                        name: "FK_medication_reminders_prescription_items_prescription_item_id",
                        column: x => x.prescription_item_id,
                        principalTable: "prescription_items",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_medication_reminders_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "medication_reminder_logs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    reminder_id = table.Column<long>(type: "bigint", nullable: false),
                    scheduled_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    completed_at = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "scheduled")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medication_reminder_logs", x => x.id);
                    table.CheckConstraint("CK_medication_reminder_logs_status", "[status] IN ('scheduled','taken','missed','skipped')");
                    table.ForeignKey(
                        name: "FK_medication_reminder_logs_medication_reminders_reminder_id",
                        column: x => x.reminder_id,
                        principalTable: "medication_reminders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UX_addresses_user_default",
                table: "addresses",
                columns: new[] { "user_id", "is_default" },
                unique: true,
                filter: "[is_default] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ai_insights_user_id",
                table: "ai_insights",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_actor_user_id",
                table: "audit_logs",
                column: "actor_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_cart_items_medication_id",
                table: "cart_items",
                column: "medication_id");

            migrationBuilder.CreateIndex(
                name: "UQ_cart_items_user_medication",
                table: "cart_items",
                columns: new[] { "user_id", "medication_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_categories_parent_id",
                table: "categories",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "UQ_categories_slug",
                table: "categories",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_diagnostic_messages_session_id",
                table: "diagnostic_messages",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "IX_diagnostic_result_medications_medication_id",
                table: "diagnostic_result_medications",
                column: "medication_id");

            migrationBuilder.CreateIndex(
                name: "UQ_diagnostic_result_medications",
                table: "diagnostic_result_medications",
                columns: new[] { "result_id", "medication_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_diagnostic_results_user_diagnosed_at",
                table: "diagnostic_results",
                columns: new[] { "user_id", "diagnosed_at" });

            migrationBuilder.CreateIndex(
                name: "UQ_diagnostic_results_session_id",
                table: "diagnostic_results",
                column: "session_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_diagnostic_session_symptoms_symptom_id",
                table: "diagnostic_session_symptoms",
                column: "symptom_id");

            migrationBuilder.CreateIndex(
                name: "UQ_diagnostic_session_symptoms",
                table: "diagnostic_session_symptoms",
                columns: new[] { "session_id", "symptom_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_diagnostic_sessions_user_id",
                table: "diagnostic_sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_health_metrics_user_type_recorded_at",
                table: "health_metrics",
                columns: new[] { "user_id", "metric_type", "recorded_at" });

            migrationBuilder.CreateIndex(
                name: "IX_medication_reminder_logs_reminder_scheduled_at",
                table: "medication_reminder_logs",
                columns: new[] { "reminder_id", "scheduled_at" });

            migrationBuilder.CreateIndex(
                name: "IX_medication_reminders_prescription_item_id",
                table: "medication_reminders",
                column: "prescription_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_medication_reminders_user_id",
                table: "medication_reminders",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_medications_category_id",
                table: "medications",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "UQ_medications_sku",
                table: "medications",
                column: "sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notifications_user_read",
                table: "notifications",
                columns: new[] { "user_id", "is_read" });

            migrationBuilder.CreateIndex(
                name: "IX_order_items_medication_id",
                table: "order_items",
                column: "medication_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_order_id",
                table: "order_items",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_address_id",
                table: "orders",
                column: "address_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_payment_method_id",
                table: "orders",
                column: "payment_method_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_prescription_id",
                table: "orders",
                column: "prescription_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_user_created_at",
                table: "orders",
                columns: new[] { "user_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "UQ_orders_order_code",
                table: "orders",
                column: "order_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_payment_methods_user_default",
                table: "payment_methods",
                columns: new[] { "user_id", "is_default" },
                unique: true,
                filter: "[is_default] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_payment_transactions_order_id",
                table: "payment_transactions",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_payment_transactions_payment_method_id",
                table: "payment_transactions",
                column: "payment_method_id");

            migrationBuilder.CreateIndex(
                name: "IX_pharmacist_chat_messages_session_id",
                table: "pharmacist_chat_messages",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "IX_pharmacist_chat_sessions_pharmacist_id",
                table: "pharmacist_chat_sessions",
                column: "pharmacist_id");

            migrationBuilder.CreateIndex(
                name: "IX_pharmacist_chat_sessions_user_id",
                table: "pharmacist_chat_sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "UQ_pharmacists_license_number",
                table: "pharmacists",
                column: "license_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_prescription_documents_prescription_id",
                table: "prescription_documents",
                column: "prescription_id");

            migrationBuilder.CreateIndex(
                name: "IX_prescription_documents_verified_by_pharmacist_id",
                table: "prescription_documents",
                column: "verified_by_pharmacist_id");

            migrationBuilder.CreateIndex(
                name: "IX_prescription_items_medication_id",
                table: "prescription_items",
                column: "medication_id");

            migrationBuilder.CreateIndex(
                name: "IX_prescription_items_prescription_id",
                table: "prescription_items",
                column: "prescription_id");

            migrationBuilder.CreateIndex(
                name: "IX_prescriptions_doctor_id",
                table: "prescriptions",
                column: "doctor_id");

            migrationBuilder.CreateIndex(
                name: "IX_prescriptions_user_id",
                table: "prescriptions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "UQ_symptoms_name",
                table: "symptoms",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_consents_user_id",
                table: "user_consents",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "UQ_user_settings_user_id",
                table: "user_settings",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_users_auth_provider_id",
                table: "users",
                column: "auth_provider_id",
                unique: true,
                filter: "[auth_provider_id] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_insights");

            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "cart_items");

            migrationBuilder.DropTable(
                name: "diagnostic_messages");

            migrationBuilder.DropTable(
                name: "diagnostic_result_medications");

            migrationBuilder.DropTable(
                name: "diagnostic_session_symptoms");

            migrationBuilder.DropTable(
                name: "health_metrics");

            migrationBuilder.DropTable(
                name: "medication_reminder_logs");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "order_items");

            migrationBuilder.DropTable(
                name: "payment_transactions");

            migrationBuilder.DropTable(
                name: "pharmacist_chat_messages");

            migrationBuilder.DropTable(
                name: "prescription_documents");

            migrationBuilder.DropTable(
                name: "user_consents");

            migrationBuilder.DropTable(
                name: "user_settings");

            migrationBuilder.DropTable(
                name: "diagnostic_results");

            migrationBuilder.DropTable(
                name: "symptoms");

            migrationBuilder.DropTable(
                name: "medication_reminders");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "pharmacist_chat_sessions");

            migrationBuilder.DropTable(
                name: "diagnostic_sessions");

            migrationBuilder.DropTable(
                name: "prescription_items");

            migrationBuilder.DropTable(
                name: "addresses");

            migrationBuilder.DropTable(
                name: "payment_methods");

            migrationBuilder.DropTable(
                name: "pharmacists");

            migrationBuilder.DropTable(
                name: "medications");

            migrationBuilder.DropTable(
                name: "prescriptions");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "doctors");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
