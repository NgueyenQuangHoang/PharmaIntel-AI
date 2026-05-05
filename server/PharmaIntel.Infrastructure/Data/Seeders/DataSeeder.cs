// =============================================================================
// Seeder: DataSeeder
// Chuc nang: Seed du lieu mau cho moi truong dev/demo.
// Nhom seed: Symptoms | Categories | Medications | Doctors | Demo User + Address + PaymentMethod.
// Idempotent: moi nhom check `if (!await _db.Xxx.AnyAsync())` -> chay nhieu lan khong nhan ban.
// =============================================================================
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PharmaIntel.Core.Entities;
using PharmaIntel.Infrastructure.Services.Helpers;

namespace PharmaIntel.Infrastructure.Data.Seeders;

public class DataSeeder
{
    public const string DemoEmail = "demo@pharmaintel.ai";
    public const string DemoPassword = "Demo@1234";
    public const string AdminEmail = "admin@pharmaintel.ai";
    public const string AdminPassword = "Admin@1234";

    private readonly PharmaIntelDbContext _db;
    private readonly IPasswordHasher<User> _hasher;

    public DataSeeder(PharmaIntelDbContext db, IPasswordHasher<User> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await SeedSymptomsAsync(ct);
        await SeedCategoriesAsync(ct);
        await SeedMedicationsAsync(ct);
        await SeedExtendedMedicationsAsync(ct);
        await SeedDoctorsAsync(ct);
        await SeedAdminUserAsync(ct);
        await SeedDemoUserAsync(ct);
    }

    // -------------------------------------------------------------------------
    // Symptoms
    // -------------------------------------------------------------------------
    private async Task SeedSymptomsAsync(CancellationToken ct)
    {
        // Desired list - day du dau tieng Viet, mo rong them nhom Da lieu va Co xuong khop.
        // Idempotent upsert: map ten cu (khong dau) -> ten moi (co dau) de nang cap database
        // hien huu ma khong xoa ID (bao toan FK tu DiagnosticSessionSymptom).
        var desired = new (string Name, string GroupName, int Order, string LegacyName)[]
        {
            // Toan than
            ("Sốt",            "Toàn thân",   1, "Sot"),
            ("Nhức mỏi",       "Toàn thân",   2, "Nhuc moi"),
            ("Chóng mặt",      "Toàn thân",   3, "Chong mat"),
            ("Mệt mỏi",        "Toàn thân",   4, "__new_met_moi"),
            ("Ớn lạnh",        "Toàn thân",   5, "__new_on_lanh"),

            // Ho hap
            ("Ho",             "Hô hấp",      1, "Ho"),
            ("Sổ mũi",         "Hô hấp",      2, "So mui"),
            ("Đau họng",       "Hô hấp",      3, "Dau hong"),
            ("Khó thở",        "Hô hấp",      4, "Kho tho"),
            ("Hắt hơi",        "Hô hấp",      5, "__new_hat_hoi"),
            ("Đờm nhiều",      "Hô hấp",      6, "__new_dom_nhieu"),

            // Than kinh
            ("Đau đầu",        "Thần kinh",   1, "Dau dau"),
            ("Mất ngủ",        "Thần kinh",   2, "__new_mat_ngu"),
            ("Hoa mắt",        "Thần kinh",   3, "__new_hoa_mat"),

            // Tieu hoa
            ("Buồn nôn",       "Tiêu hóa",    1, "Buon non"),
            ("Đau bụng",       "Tiêu hóa",    2, "Dau bung"),
            ("Tiêu chảy",      "Tiêu hóa",    3, "Tieu chay"),
            ("Táo bón",        "Tiêu hóa",    4, "__new_tao_bon"),
            ("Đầy hơi",        "Tiêu hóa",    5, "__new_day_hoi"),
            ("Ợ chua",         "Tiêu hóa",    6, "__new_o_chua"),

            // Tim mach
            ("Đau ngực",       "Tim mạch",    1, "Dau nguc"),
            ("Hồi hộp",        "Tim mạch",    2, "__new_hoi_hop"),
            ("Tăng huyết áp",  "Tim mạch",    3, "__new_tang_huyet_ap"),

            // Da lieu (moi)
            ("Mẩn ngứa",       "Da liễu",     1, "__new_man_ngua"),
            ("Phát ban",       "Da liễu",     2, "__new_phat_ban"),
            ("Nổi mề đay",     "Da liễu",     3, "__new_noi_me_day"),

            // Co xuong khop (moi)
            ("Đau lưng",       "Cơ xương khớp", 1, "__new_dau_lung"),
            ("Đau khớp",       "Cơ xương khớp", 2, "__new_dau_khop"),
            ("Co cứng cơ",     "Cơ xương khớp", 3, "__new_co_cung_co"),
        };

        var existing = await _db.Symptoms.ToListAsync(ct);

        foreach (var d in desired)
        {
            // Tim theo legacy name (data cu khong dau) hoac theo name moi (data da nang cap)
            var match = existing.FirstOrDefault(x =>
                string.Equals(x.Name, d.LegacyName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(x.Name, d.Name, StringComparison.Ordinal));

            if (match == null)
            {
                _db.Symptoms.Add(new Symptom
                {
                    Name = d.Name,
                    GroupName = d.GroupName,
                    DisplayOrder = d.Order
                });
            }
            else
            {
                match.Name = d.Name;
                match.GroupName = d.GroupName;
                match.DisplayOrder = d.Order;
            }
        }

        await _db.SaveChangesAsync(ct);
    }

    // -------------------------------------------------------------------------
    // Categories - flat list 8 danh muc chinh.
    // Idempotent per-slug: chay duoc tren ca DB fresh va DB hien huu.
    // -------------------------------------------------------------------------
    private async Task SeedCategoriesAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var seedItems = new[]
        {
            new { Name = "Thuốc",                 Order = 1 },
            new { Name = "Thuốc Cảm",             Order = 2 },
            new { Name = "Kháng Sinh",            Order = 3 },
            new { Name = "Mắt",                   Order = 4 },
            new { Name = "Tim mạch / huyết áp",   Order = 5 },
            new { Name = "Xương khớp",            Order = 6 },
            new { Name = "Thực phẩm chức năng",   Order = 7 },
            new { Name = "Dược mỹ phẩm",          Order = 8 },
            new { Name = "Thiết bị y tế",         Order = 9 },
            new { Name = "Vật tư y tế",           Order = 10 },
            new { Name = "Chăm sóc cá nhân",      Order = 11 },
            new { Name = "Mẹ và bé",              Order = 12 },
            new { Name = "Đông y & thảo dược",    Order = 13 },
        };

        foreach (var s in seedItems)
        {
            var slug = SlugHelper.ToSlug(s.Name);
            if (await _db.Categories.AnyAsync(c => c.Slug == slug, ct)) continue;

            _db.Categories.Add(new Category
            {
                Name = s.Name,
                Slug = slug,
                DisplayOrder = s.Order,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
            });
        }
        await _db.SaveChangesAsync(ct);
    }

    // -------------------------------------------------------------------------
    // Medications (12 OTC mau)
    // -------------------------------------------------------------------------
    private async Task SeedMedicationsAsync(CancellationToken ct)
    {
        if (await _db.Medications.AnyAsync(ct)) return;

        // Lookup CategoryId theo slug de gan FK
        var cats = await _db.Categories.AsNoTracking().ToDictionaryAsync(c => c.Slug, c => c.Id, ct);
        long Cat(string slug) => cats[slug];

        var thuocSlug = SlugHelper.ToSlug("Thuốc");
        var tpcnSlug  = SlugHelper.ToSlug("Thực phẩm chức năng");

        var now = DateTime.UtcNow;
        var meds = new[]
        {
            new Medication { Sku = "MED-PARA-500", Name = "Paracetamol 500mg",   GenericName = "Paracetamol", CategoryId = Cat(thuocSlug),
                Manufacturer = "Stada", Price = 15000m, DiscountPercent = 5m, StockQuantity = 500, Packaging = "Hop 10 vi x 10 vien",
                ActiveIngredients = "Paracetamol 500mg", UsageInstructions = "Uong 1-2 vien moi 4-6 gio khi sot/dau, khong qua 8 vien/ngay.",
                IsFeatured = true, IsBestSeller = true, IsPrescriptionRequired = false, IsActive = true, CreatedAt = now, UpdatedAt = now },

            new Medication { Sku = "MED-IBU-400",  Name = "Ibuprofen 400mg",      GenericName = "Ibuprofen", CategoryId = Cat(thuocSlug),
                Manufacturer = "Hau Giang", Price = 25000m, DiscountPercent = 0m, StockQuantity = 300, Packaging = "Hop 3 vi x 10 vien",
                ActiveIngredients = "Ibuprofen 400mg", UsageInstructions = "Uong 1 vien moi 6-8 gio sau an.",
                IsBestSeller = true, IsPrescriptionRequired = false, IsActive = true, CreatedAt = now, UpdatedAt = now },

            new Medication { Sku = "MED-DECOL",    Name = "Decolgen Forte",       CategoryId = Cat(thuocSlug),
                Manufacturer = "United Pharma", Price = 32000m, DiscountPercent = 10m, StockQuantity = 250, Packaging = "Hop 25 vi x 4 vien",
                ActiveIngredients = "Paracetamol + Phenylephrin + Chlorpheniramin", UsageInstructions = "Uong 1 vien khi co trieu chung cam cum.",
                IsFeatured = true, IsPrescriptionRequired = false, IsActive = true, CreatedAt = now, UpdatedAt = now },

            new Medication { Sku = "MED-STREP",    Name = "Strepsils huong mat",  CategoryId = Cat(thuocSlug),
                Manufacturer = "Reckitt", Price = 45000m, DiscountPercent = 0m, StockQuantity = 200, Packaging = "Hop 24 vien ngam",
                UsageInstructions = "Ngam 1 vien moi 2-3 gio khi dau hong.",
                IsPrescriptionRequired = false, IsActive = true, CreatedAt = now, UpdatedAt = now },

            new Medication { Sku = "MED-LORAT",    Name = "Loratadine 10mg",      CategoryId = Cat(thuocSlug),
                Manufacturer = "DHG", Price = 28000m, DiscountPercent = 0m, StockQuantity = 180, Packaging = "Hop 1 vi x 10 vien",
                ActiveIngredients = "Loratadine 10mg", UsageInstructions = "Uong 1 vien moi ngay khi di ung / so mui.",
                IsPrescriptionRequired = false, IsActive = true, CreatedAt = now, UpdatedAt = now },

            new Medication { Sku = "MED-SMEC",     Name = "Smecta",               CategoryId = Cat(thuocSlug),
                Manufacturer = "Ipsen", Price = 65000m, DiscountPercent = 5m, StockQuantity = 150, Packaging = "Hop 30 goi bot",
                ActiveIngredients = "Diosmectite", UsageInstructions = "Pha 1 goi voi 50ml nuoc, uong sau bua an.",
                IsBestSeller = true, IsPrescriptionRequired = false, IsActive = true, CreatedAt = now, UpdatedAt = now },

            new Medication { Sku = "MED-BERB",     Name = "Berberin 50mg",        CategoryId = Cat(thuocSlug),
                Manufacturer = "Mekophar", Price = 18000m, DiscountPercent = 0m, StockQuantity = 220, Packaging = "Lo 100 vien",
                UsageInstructions = "Uong 4-6 vien/ngay khi tieu chay nhe.",
                IsPrescriptionRequired = false, IsActive = true, CreatedAt = now, UpdatedAt = now },

            new Medication { Sku = "MED-DOMP",     Name = "Domperidon 10mg",      CategoryId = Cat(thuocSlug),
                Manufacturer = "Sanofi", Price = 22000m, DiscountPercent = 0m, StockQuantity = 140, Packaging = "Hop 3 vi x 10 vien",
                ActiveIngredients = "Domperidon", UsageInstructions = "Uong 1 vien truoc bua an 15-30 phut khi buon non.",
                IsPrescriptionRequired = false, IsActive = true, CreatedAt = now, UpdatedAt = now },

            new Medication { Sku = "MED-ENTERO",   Name = "Enterogermina",        CategoryId = Cat(thuocSlug),
                Manufacturer = "Sanofi", Price = 95000m, DiscountPercent = 8m, StockQuantity = 160, Packaging = "Hop 20 ong x 5ml",
                UsageInstructions = "Uong 1-2 ong/ngay de ho tro he tieu hoa.",
                IsFeatured = true, IsPrescriptionRequired = false, IsActive = true, CreatedAt = now, UpdatedAt = now },

            new Medication { Sku = "MED-ORE",      Name = "Oresol bu dien giai",  CategoryId = Cat(thuocSlug),
                Manufacturer = "Bidipharm", Price = 8000m, DiscountPercent = 0m, StockQuantity = 400, Packaging = "Goi 27.9g pha 1 lit",
                UsageInstructions = "Pha 1 goi voi 1 lit nuoc dun soi de nguoi, uong dan trong 24h.",
                IsPrescriptionRequired = false, IsActive = true, CreatedAt = now, UpdatedAt = now },

            new Medication { Sku = "MED-VITC",     Name = "Vitamin C 1000mg",     CategoryId = Cat(tpcnSlug),
                Manufacturer = "Bayer", Price = 120000m, DiscountPercent = 12m, StockQuantity = 280, Packaging = "Tup 20 vien sui",
                UsageInstructions = "Hoa tan 1 vien sui voi 200ml nuoc, uong moi sang.",
                IsFeatured = true, IsBestSeller = true, IsPrescriptionRequired = false, IsActive = true, CreatedAt = now, UpdatedAt = now },

            new Medication { Sku = "MED-ZINC",     Name = "Kem gluconate 70mg",   CategoryId = Cat(tpcnSlug),
                Manufacturer = "Mediplantex", Price = 55000m, DiscountPercent = 0m, StockQuantity = 130, Packaging = "Hop 30 vien",
                UsageInstructions = "Uong 1 vien/ngay sau bua an.",
                IsPrescriptionRequired = false, IsActive = true, CreatedAt = now, UpdatedAt = now }
        };

        _db.Medications.AddRange(meds);
        await _db.SaveChangesAsync(ct);
    }

    // -------------------------------------------------------------------------
    // Extended Medications (118 san pham mau theo categoryIdMapping cua user)
    // Idempotent per-SKU: chi them san pham co SKU chua ton tai trong DB.
    // Category lookup theo slug -> CategoryId thuc te (khong hardcode ID).
    // -------------------------------------------------------------------------
    private async Task SeedExtendedMedicationsAsync(CancellationToken ct)
    {
        var cats = await _db.Categories.AsNoTracking().ToDictionaryAsync(c => c.Slug, c => c.Id, ct);
        long Cat(string slug) => cats[slug];

        // (Sku, Name, GenericName, Manufacturer, Price, DiscountPct, CategorySlug, IsFeatured, IsBestSeller, IsRx, Stock)
        var seed = new (string Sku, string Name, string? Generic, string? Mfr, decimal Price, decimal Discount, string Slug, bool Featured, bool BestSeller, bool Rx, int Stock)[]
        {
            // Thuoc
            ("TH-PANACTOL-500MG-001", "Panactol 500mg", "Paracetamol", "Khapharco", 40000m, 0m, "thuoc", false, true, false, 35),
            ("TH-EFFERALGAN-500MG-002", "Efferalgan 500mg", "Paracetamol", "UPSA / Bristol Myers Squibb", 60000m, 0m, "thuoc", false, true, false, 40),
            ("TH-PANADOL-EXTRA-003", "Panadol Extra", "Paracetamol + Caffeine", "Haleon", 190000m, 0m, "thuoc", false, true, false, 45),
            ("TH-HAPACOL-650-004", "Hapacol 650", "Paracetamol", "DHG Pharma", 55000m, 0m, "thuoc", false, true, false, 50),
            ("TH-SMECTA-005", "Smecta", "Diosmectite", "Ipsen", 135000m, 0m, "thuoc", false, true, false, 55),
            ("TH-ORESOL-245-006", "Oresol 245", "Oral Rehydration Salts", "OPC / nhiều NSX", 45000m, 0m, "thuoc", false, false, false, 60),
            ("TH-GAVISCON-DUAL-ACTION-007", "Gaviscon Dual Action", "Sodium alginate + antacid", "Reckitt", 185000m, 0m, "thuoc", true, false, false, 65),
            ("TH-BETADINE-ANTISEPTIC-SOLU-008", "Betadine Antiseptic Solution 10%", "Povidone-iodine", "Mundipharma", 55000m, 0m, "thuoc", false, true, false, 70),
            ("TH-SALONPAS-PAIN-RELIEF-PAT-009", "Salonpas Pain Relief Patch", "Methyl salicylate + Menthol", "Hisamitsu", 38000m, 0m, "thuoc", false, true, false, 75),
            ("TH-DA-HUONG-TRA-XANH-010", "Dạ Hương Trà Xanh", "Dung dịch vệ sinh phụ nữ", "Hoa Linh", 42000m, 0m, "thuoc", false, false, false, 80),

            // Thuoc Cam (bo qua Panadol Cold & Flu va Tiffy Dey theo yeu cau)
            ("TC-DECOLGEN-FORTE-001", "Decolgen Forte", "Paracetamol + Phenylephrine + Chlorpheniramine", "United Pharma", 120000m, 0m, "thuoc-cam", false, true, false, 35),
            ("TC-HAPACOL-CAM-CUM-002", "Hapacol Cảm Cúm", "Paracetamol + Chlorpheniramine + Phenylephrine", "DHG Pharma", 85000m, 0m, "thuoc-cam", false, true, false, 40),
            ("TC-AMEFLU-DAYTIME-003", "Ameflu Daytime", "Paracetamol + Phenylephrine + Dextromethorphan", "OPV", 95000m, 0m, "thuoc-cam", false, true, false, 45),
            ("TC-AMEFLU-NIGHTTIME-004", "Ameflu Nighttime", "Paracetamol + Chlorpheniramine + Dextromethorphan", "OPV", 98000m, 0m, "thuoc-cam", false, false, false, 50),
            ("TC-COLDACMIN-FLU-005", "Coldacmin Flu", "Paracetamol + Chlorpheniramine + Phenylephrine", "Tipharco / nhiều NSX", 65000m, 0m, "thuoc-cam", false, false, false, 55),
            ("TC-RHUMENOL-FLU-500-006", "Rhumenol Flu 500", "Paracetamol + Loratadine + Dextromethorphan", "Roussel Việt Nam / nhiều NSX", 70000m, 0m, "thuoc-cam", false, false, false, 60),
            ("TC-ANTIFLU-CAPSULES-007", "Antiflu Capsules", "Paracetamol + Chlorpheniramine + Phenylephrine", "Nhiều nhà sản xuất", 62000m, 0m, "thuoc-cam", false, false, false, 65),
            ("TC-PROSPAN-SYRUP-008", "Prospan Syrup", "Cao lá thường xuân", "Engelhard Arzneimittel", 115000m, 0m, "thuoc-cam", false, true, false, 70),

            // Khang Sinh
            ("KS-AUGMENTIN-625MG-001", "Augmentin 625mg", "Amoxicillin + Acid clavulanic", "GSK", 120000m, 0m, "khang-sinh", false, false, true, 35),
            ("KS-KLAMENTIN-625-002", "Klamentin 625", "Amoxicillin + Acid clavulanic", "DHG Pharma", 120000m, 0m, "khang-sinh", false, false, true, 40),
            ("KS-ZINNAT-500MG-003", "Zinnat 500mg", "Cefuroxime axetil", "GSK / nhiều NSX", 120000m, 0m, "khang-sinh", false, false, true, 45),
            ("KS-CEFIXIM-200MG-004", "Cefixim 200mg", "Cefixime", "Nhiều nhà sản xuất", 120000m, 0m, "khang-sinh", false, false, true, 50),
            ("KS-AZITHROMYCIN-500MG-005", "Azithromycin 500mg", "Azithromycin", "Nhiều nhà sản xuất", 120000m, 0m, "khang-sinh", false, false, true, 55),
            ("KS-CIPROFLOXACIN-500MG-006", "Ciprofloxacin 500mg", "Ciprofloxacin", "Nhiều nhà sản xuất", 120000m, 0m, "khang-sinh", false, false, true, 60),
            ("KS-DOXYCYCLINE-100MG-007", "Doxycycline 100mg", "Doxycycline", "Nhiều nhà sản xuất", 120000m, 0m, "khang-sinh", false, false, true, 65),
            ("KS-ERYTHROMYCIN-500MG-008", "Erythromycin 500mg", "Erythromycin", "Nhiều nhà sản xuất", 120000m, 0m, "khang-sinh", false, false, true, 70),
            ("KS-CEPHALEXIN-500MG-009", "Cephalexin 500mg", "Cephalexin", "Nhiều nhà sản xuất", 120000m, 0m, "khang-sinh", false, false, true, 75),
            ("KS-METRONIDAZOLE-250MG-010", "Metronidazole 250mg", "Metronidazole", "Nhiều nhà sản xuất", 120000m, 0m, "khang-sinh", false, false, true, 80),

            // Mat
            ("MAT-SYSTANE-ULTRA-10ML-001", "Systane Ultra 10ml", "Polyethylene glycol 400 + Propylene glycol", "Alcon", 90000m, 0m, "mat", false, false, false, 35),
            ("MAT-REFRESH-TEARS-15ML-002", "Refresh Tears 15ml", "Carboxymethylcellulose sodium", "Allergan / AbbVie", 90000m, 0m, "mat", false, false, false, 40),
            ("MAT-V-ROHTO-VITAMIN-003", "V.Rohto Vitamin", "Dung dịch nhỏ mắt vitamin", "Rohto-Mentholatum", 90000m, 0m, "mat", false, false, false, 45),
            ("MAT-TOBREX-0-3-EYE-DROPS-004", "Tobrex 0.3% Eye Drops", "Tobramycin", "Novartis / Alcon", 140000m, 0m, "mat", false, false, true, 50),
            ("MAT-CRAVIT-0-5-EYE-DROPS-005", "Cravit 0.5% Eye Drops", "Levofloxacin", "Santen", 140000m, 0m, "mat", false, false, true, 55),
            ("MAT-TOBRADEX-EYE-DROPS-006", "Tobradex Eye Drops", "Tobramycin + Dexamethasone", "Novartis / Alcon", 140000m, 0m, "mat", false, false, true, 60),
            ("MAT-OFLOVID-EYE-DROPS-007", "Oflovid Eye Drops", "Ofloxacin", "Santen", 140000m, 0m, "mat", false, false, true, 65),
            ("MAT-SANLEIN-0-1-EYE-DROPS-008", "Sanlein 0.1 Eye Drops", "Sodium hyaluronate", "Santen", 90000m, 0m, "mat", false, false, false, 70),
            ("MAT-EYEMIRU-40EX-009", "Eyemiru 40EX", "Dung dịch nhỏ mắt vitamin", "Santen / Nhật Bản", 90000m, 0m, "mat", false, false, false, 75),
            ("MAT-OPTIVE-FUSION-010", "Optive Fusion", "Carboxymethylcellulose + Glycerin + Sodium hyaluronate", "Allergan / AbbVie", 90000m, 0m, "mat", false, false, false, 80),

            // Tim mach / huyet ap
            ("TM-AMLOR-5MG-001", "Amlor 5mg", "Amlodipine", "Viatris / Pfizer", 160000m, 0m, "tim-mach-huyet-ap", false, false, true, 35),
            ("TM-NORVASC-5MG-002", "Norvasc 5mg", "Amlodipine", "Pfizer", 160000m, 0m, "tim-mach-huyet-ap", false, false, true, 40),
            ("TM-COVERSYL-5MG-003", "Coversyl 5mg", "Perindopril arginine", "Servier", 160000m, 0m, "tim-mach-huyet-ap", false, false, true, 45),
            ("TM-CONCOR-5MG-004", "Concor 5mg", "Bisoprolol fumarate", "Merck", 160000m, 0m, "tim-mach-huyet-ap", false, false, true, 50),
            ("TM-MICARDIS-40MG-005", "Micardis 40mg", "Telmisartan", "Boehringer Ingelheim", 160000m, 0m, "tim-mach-huyet-ap", false, false, true, 55),
            ("TM-COZAAR-50MG-006", "Cozaar 50mg", "Losartan potassium", "Organon / MSD", 160000m, 0m, "tim-mach-huyet-ap", false, false, true, 60),
            ("TM-PLAVIX-75MG-007", "Plavix 75mg", "Clopidogrel", "Sanofi", 160000m, 0m, "tim-mach-huyet-ap", false, false, true, 65),
            ("TM-ASPIRIN-81MG-008", "Aspirin 81mg", "Acetylsalicylic acid", "Bayer / nhiều NSX", 160000m, 0m, "tim-mach-huyet-ap", false, false, true, 70),
            ("TM-ATORVASTATIN-20MG-009", "Atorvastatin 20mg", "Atorvastatin", "Nhiều nhà sản xuất", 160000m, 0m, "tim-mach-huyet-ap", false, false, true, 75),
            ("TM-ROSUVASTATIN-10MG-010", "Rosuvastatin 10mg", "Rosuvastatin", "Nhiều nhà sản xuất", 160000m, 0m, "tim-mach-huyet-ap", false, false, true, 80),

            // Xuong khop
            ("XK-VOLTAREN-EMULGEL-1-001", "Voltaren Emulgel 1%", "Diclofenac diethylamine", "GSK / Haleon", 180000m, 0m, "xuong-khop", false, false, false, 35),
            ("XK-MOBIC-7-5MG-002", "Mobic 7.5mg", "Meloxicam", "Boehringer Ingelheim", 180000m, 0m, "xuong-khop", false, false, true, 40),
            ("XK-CELEBREX-200MG-003", "Celebrex 200mg", "Celecoxib", "Pfizer", 180000m, 0m, "xuong-khop", false, false, true, 45),
            ("XK-GLUCOSAMINE-1500MG-004", "Glucosamine 1500mg", "Glucosamine sulfate", "Nhiều thương hiệu", 180000m, 0m, "xuong-khop", false, false, false, 50),
            ("XK-OSTEOCARE-ORIGINAL-005", "Osteocare Original", "Calcium + Magnesium + Zinc + Vitamin D", "Vitabiotics", 180000m, 0m, "xuong-khop", false, false, false, 55),
            ("XK-CALTRATE-600-D3-006", "Caltrate 600+D3", "Calcium + Vitamin D3", "Haleon", 180000m, 0m, "xuong-khop", false, false, false, 60),
            ("XK-DOPPELHERZ-AKTIV-GLUCOSA-007", "Doppelherz Aktiv Glucosamine 1500", "Glucosamine + Chondroitin", "Queisser Pharma", 180000m, 0m, "xuong-khop", false, false, false, 65),
            ("XK-JEX-MAX-008", "Jex Max", "Peptan + thảo dược", "Eco Pharma", 180000m, 0m, "xuong-khop", false, false, false, 70),
            ("XK-SALONPAS-GEL-009", "Salonpas Gel", "Methyl salicylate + Menthol", "Hisamitsu", 180000m, 0m, "xuong-khop", false, false, false, 75),
            ("XK-ALPHA-CHOAY-010", "Alpha Choay", "Alpha chymotrypsin", "Sanofi", 180000m, 0m, "xuong-khop", false, false, true, 80),

            // Thuc pham chuc nang
            ("TPCN-BLACKMORES-BIO-C-1000MG-001", "Blackmores Bio C 1000mg", "Vitamin C 1000mg, rosehip, bioflavonoids, rutin, hesperidin, acerola", "Blackmores", 280000m, 0m, "thuc-pham-chuc-nang", false, false, false, 35),
            ("TPCN-BEROCCA-PERFORMANCE-002", "Berocca Performance", "Vitamin C, vitamin nhóm B, calci, magie, kẽm", "Bayer", 75000m, 0m, "thuc-pham-chuc-nang", false, false, false, 40),
            ("TPCN-DHC-VITAMIN-C-003", "DHC Vitamin C", "Vitamin C, vitamin B2", "DHC", 120000m, 0m, "thuc-pham-chuc-nang", false, false, false, 45),
            ("TPCN-DOPPELHERZ-A-Z-DEPOT-004", "Doppelherz A-Z Depot", "Vitamin và khoáng chất tổng hợp", "Queisser Pharma", 230000m, 0m, "thuc-pham-chuc-nang", false, false, false, 50),
            ("TPCN-NATURE-MADE-FISH-OIL-120-005", "Nature Made Fish Oil 1200mg", "Dầu cá chứa omega-3 EPA/DHA", "Nature Made", 360000m, 0m, "thuc-pham-chuc-nang", false, false, false, 55),
            ("TPCN-KIRKLAND-CALCIUM-600MG-D-006", "Kirkland Calcium 600mg + D3", "Calcium + Vitamin D3", "Kirkland Signature", 430000m, 0m, "thuc-pham-chuc-nang", false, false, false, 60),
            ("TPCN-HEALTHY-CARE-COENZYME-Q1-007", "Healthy Care CoEnzyme Q10 150mg", "Coenzyme Q10", "Healthy Care", 420000m, 0m, "thuc-pham-chuc-nang", false, false, false, 65),
            ("TPCN-SWISSE-ULTIBOOST-LIVER-D-008", "Swisse Ultiboost Liver Detox", "Chiết xuất kế sữa, atisô, nghệ", "Swisse", 380000m, 0m, "thuc-pham-chuc-nang", false, false, false, 70),
            ("TPCN-DOPPELHERZ-AKTIV-KINDER--009", "Doppelherz Aktiv Kinder Immune", "Vitamin C, D, kẽm và vi chất", "Queisser Pharma", 210000m, 0m, "thuc-pham-chuc-nang", false, false, false, 75),
            ("TPCN-IMMUNOGLUCAN-P4H-010", "ImmunoGlucan P4H", "Beta glucan, vitamin C", "Pleuran", 320000m, 0m, "thuc-pham-chuc-nang", false, false, false, 80),

            // Duoc my pham
            ("DMP-CERAVE-HYDRATING-CLEANSE-001", "CeraVe Hydrating Cleanser 236ml", "Ceramides, hyaluronic acid", "CeraVe", 320000m, 0m, "duoc-my-pham", false, false, false, 35),
            ("DMP-LA-ROCHE-POSAY-EFFACLAR--002", "La Roche-Posay Effaclar Duo+M 40ml", "Phylobioma, niacinamide và thành phần hỗ trợ giảm mụn", "La Roche-Posay", 450000m, 0m, "duoc-my-pham", false, false, false, 40),
            ("DMP-LA-ROCHE-POSAY-ANTHELIOS-003", "La Roche-Posay Anthelios UVMune 400 SPF50+", "Bộ lọc chống nắng quang phổ rộng", "La Roche-Posay", 520000m, 0m, "duoc-my-pham", false, false, false, 45),
            ("DMP-BIODERMA-SENSIBIO-H2O-50-004", "Bioderma Sensibio H2O 500ml", "Micellar water cho da nhạy cảm", "Bioderma", 390000m, 0m, "duoc-my-pham", false, false, false, 50),
            ("DMP-EUCERIN-PROACNE-SOLUTION-005", "Eucerin ProAcne Solution Cleansing Gel", "Cleansing gel không xà phòng", "Eucerin", 330000m, 0m, "duoc-my-pham", false, false, false, 55),
            ("DMP-AVENE-CICALFATE-REPAIRIN-006", "Avene Cicalfate+ Repairing Protective Cream", "Sucralfate, đồng-kẽm sulfate", "Avène", 360000m, 0m, "duoc-my-pham", false, false, false, 60),
            ("DMP-SVR-SEBIACLEAR-GEL-MOUSS-007", "SVR Sebiaclear Gel Moussant", "Gluconolactone, chất làm sạch dịu nhẹ", "SVR", 300000m, 0m, "duoc-my-pham", false, false, false, 65),
            ("DMP-VICHY-MINERAL-89-008", "Vichy Mineral 89", "Nước khoáng Vichy, hyaluronic acid", "Vichy", 650000m, 0m, "duoc-my-pham", false, false, false, 70),
            ("DMP-CETAPHIL-GENTLE-SKIN-CLE-009", "Cetaphil Gentle Skin Cleanser", "Công thức làm sạch dịu nhẹ", "Cetaphil", 250000m, 0m, "duoc-my-pham", false, false, false, 75),
            ("DMP-BEPANTHEN-BALM-010", "Bepanthen Balm", "Dexpanthenol", "Bayer", 160000m, 0m, "duoc-my-pham", false, false, false, 80),

            // Cham soc ca nhan
            ("CSCN-DA-HUONG-LAVENDER-100ML-001", "Dạ Hương Lavender 100ml", "Chiết xuất thảo dược, acid lactic", "Hoa Linh", 42000m, 0m, "cham-soc-ca-nhan", false, false, false, 35),
            ("CSCN-BETADINE-FEMININE-WASH-1-002", "Betadine Feminine Wash 100ml", "Povidone-iodine/acid lactic tùy dòng sản phẩm", "Mundipharma", 85000m, 0m, "cham-soc-ca-nhan", false, false, false, 40),
            ("CSCN-LISTERINE-COOL-MINT-500M-003", "Listerine Cool Mint 500ml", "Tinh dầu sát khuẩn, fluoride tùy dòng", "Kenvue", 95000m, 0m, "cham-soc-ca-nhan", false, false, false, 45),
            ("CSCN-P-S-KEM-DANH-RANG-SENSIT-004", "P/S Kem đánh răng Sensitive Expert", "Potassium nitrate/fluoride tùy dòng", "Unilever", 48000m, 0m, "cham-soc-ca-nhan", false, false, false, 50),
            ("CSCN-SENSODYNE-REPAIR-PROTECT-005", "Sensodyne Repair & Protect", "Stannous fluoride/Novamin tùy phiên bản", "Haleon", 98000m, 0m, "cham-soc-ca-nhan", false, false, false, 55),
            ("CSCN-DOVE-SENSITIVE-SKIN-BODY-006", "Dove Sensitive Skin Body Wash", "Chất làm sạch dịu nhẹ, dưỡng ẩm", "Unilever", 160000m, 0m, "cham-soc-ca-nhan", false, false, false, 60),
            ("CSCN-CETAPHIL-MOISTURIZING-CR-007", "Cetaphil Moisturizing Cream", "Glycerin, niacinamide/panthenol tùy công thức", "Cetaphil", 310000m, 0m, "cham-soc-ca-nhan", false, false, false, 65),
            ("CSCN-NIVEA-MEN-SILVER-PROTECT-008", "Nivea Men Silver Protect Roll On", "Thành phần khử mùi/kháng khuẩn", "Beiersdorf", 65000m, 0m, "cham-soc-ca-nhan", false, false, false, 70),
            ("CSCN-LIFEBUOY-HAND-SANITIZER-009", "Lifebuoy Hand Sanitizer", "Alcohol và chất dưỡng ẩm", "Unilever", 35000m, 0m, "cham-soc-ca-nhan", false, false, false, 75),
            ("CSCN-PHYSIOGEL-DAILY-MOISTURE-010", "Physiogel Daily Moisture Therapy Cream", "Lipid sinh học và chất dưỡng ẩm", "Kenvue / GSK", 280000m, 0m, "cham-soc-ca-nhan", false, false, false, 80),

            // Me va be
            ("MVB-PEDIAKID-VITAMIN-D3-001", "Pediakid Vitamin D3", "Vitamin D3", "Ineldea", 250000m, 0m, "me-va-be", false, false, false, 35),
            ("MVB-BABY-DDROPS-400-IU-002", "Baby Ddrops 400 IU", "Vitamin D3 400 IU/giọt", "Ddrops", 280000m, 0m, "me-va-be", false, false, false, 40),
            ("MVB-BIOGAIA-PROTECTIS-BABY-D-003", "BioGaia Protectis Baby Drops", "Lactobacillus reuteri Protectis", "BioGaia", 350000m, 0m, "me-va-be", false, false, false, 45),
            ("MVB-FYSOLINE-NUOC-MUOI-SINH--004", "Fysoline Nước muối sinh lý 5ml", "Natri clorid 0.9%", "Gifrer", 95000m, 0m, "me-va-be", false, false, false, 50),
            ("MVB-SUDOCREM-ANTISEPTIC-HEAL-005", "Sudocrem Antiseptic Healing Cream", "Zinc oxide và tá dược bảo vệ da", "Teva / Forest Tosara", 160000m, 0m, "me-va-be", false, false, false, 55),
            ("MVB-BEPANTHEN-OINTMENT-006", "Bepanthen Ointment", "Dexpanthenol", "Bayer", 170000m, 0m, "me-va-be", false, false, false, 60),
            ("MVB-SUA-APTAMIL-PROFUTURA-1-007", "Sữa Aptamil Profutura 1", "Sữa công thức theo nhãn", "Nutricia", 850000m, 0m, "me-va-be", false, false, false, 65),
            ("MVB-MEIJI-HOHOEMI-INFANT-FOR-008", "Meiji Hohoemi Infant Formula", "Sữa công thức theo nhãn", "Meiji", 520000m, 0m, "me-va-be", false, false, false, 70),
            ("MVB-HUGGIES-DRY-NEWBORN-009", "Huggies Dry Newborn", "Tã giấy sơ sinh", "Kimberly-Clark", 165000m, 0m, "me-va-be", false, false, false, 75),
            ("MVB-JOHNSON-S-BABY-TOP-TO-TO-010", "Johnson's Baby Top-to-Toe Wash", "Sữa tắm gội dịu nhẹ", "Kenvue", 90000m, 0m, "me-va-be", false, false, false, 80),

            // Thiet bi y te
            ("TBYT-MAY-DO-HUYET-AP-BAP-TAY--001", "Máy đo huyết áp bắp tay Omron HEM-7121", null, "Omron", 780000m, 0m, "thiet-bi-y-te", false, false, false, 35),
            ("TBYT-MAY-DO-DUONG-HUYET-ACCU--002", "Máy đo đường huyết Accu-Chek Instant", null, "Roche", 650000m, 0m, "thiet-bi-y-te", false, false, false, 40),
            ("TBYT-NHIET-KE-DIEN-TU-OMRON-M-003", "Nhiệt kế điện tử Omron MC-246", null, "Omron", 95000m, 0m, "thiet-bi-y-te", false, false, false, 45),
            ("TBYT-MAY-XONG-KHI-DUNG-OMRON--004", "Máy xông khí dung Omron NE-C101", null, "Omron", 950000m, 0m, "thiet-bi-y-te", false, false, false, 50),
            ("TBYT-MAY-DO-SPO2-BEURER-PO30-005", "Máy đo SpO2 Beurer PO30", null, "Beurer", 850000m, 0m, "thiet-bi-y-te", false, false, false, 55),
            ("TBYT-CAN-SUC-KHOE-DIEN-TU-TAN-006", "Cân sức khỏe điện tử Tanita HD-662", null, "Tanita", 520000m, 0m, "thiet-bi-y-te", false, false, false, 60),
            ("TBYT-NHIET-KE-HONG-NGOAI-MICR-007", "Nhiệt kế hồng ngoại Microlife FR1MF1", null, "Microlife", 890000m, 0m, "thiet-bi-y-te", false, false, false, 65),
            ("TBYT-MAY-DO-HUYET-AP-MICROLIF-008", "Máy đo huyết áp Microlife B3 Basic", null, "Microlife", 1050000m, 0m, "thiet-bi-y-te", false, false, false, 70),
            ("TBYT-MAY-MASSAGE-XUNG-DIEN-BE-009", "Máy massage xung điện Beurer EM49", null, "Beurer", 1750000m, 0m, "thiet-bi-y-te", false, false, false, 75),
            ("TBYT-DAI-NEP-CO-MEM-ORBE-010", "Đai nẹp cổ mềm Orbe", null, "Orbe", 180000m, 0m, "thiet-bi-y-te", false, false, false, 80),

            // Vat tu y te
            ("VTYT-KHAU-TRANG-Y-TE-4-LOP-001", "Khẩu trang y tế 4 lớp", "Vải không dệt, lớp lọc kháng khuẩn", "Nhiều nhà sản xuất", 35000m, 0m, "vat-tu-y-te", false, false, false, 35),
            ("VTYT-GANG-TAY-Y-TE-KHONG-BOT--002", "Găng tay y tế không bột VGlove", "Latex/nitrile tùy loại", "VRG Khải Hoàn", 120000m, 0m, "vat-tu-y-te", false, false, false, 40),
            ("VTYT-BONG-Y-TE-BAO-THACH-100G-003", "Bông y tế Bảo Thạch 100g", "Bông thấm nước", "Bảo Thạch", 28000m, 0m, "vat-tu-y-te", false, false, false, 45),
            ("VTYT-GAC-Y-TE-VO-TRUNG-10X10-004", "Gạc y tế vô trùng 10x10", "Gạc cotton vô trùng", "Nhiều nhà sản xuất", 45000m, 0m, "vat-tu-y-te", false, false, false, 50),
            ("VTYT-CON-70-DO-500ML-005", "Cồn 70 độ 500ml", "Ethanol 70%", "Nhiều nhà sản xuất", 45000m, 0m, "vat-tu-y-te", false, false, false, 55),
            ("VTYT-NUOC-MUOI-SINH-LY-NATRI--006", "Nước muối sinh lý Natri Clorid 0.9% 500ml", "Sodium chloride 0.9%", "Nhiều nhà sản xuất", 18000m, 0m, "vat-tu-y-te", false, false, false, 60),
            ("VTYT-BANG-KEO-CA-NHAN-URGO-007", "Băng keo cá nhân Urgo", "Băng dính cá nhân", "Urgo", 30000m, 0m, "vat-tu-y-te", false, false, false, 65),
            ("VTYT-KIM-TIEM-VINAHANKOOK-5ML-008", "Kim tiêm Vinahankook 5ml", "Ống tiêm/kim vô trùng", "Vinahankook", 65000m, 0m, "vat-tu-y-te", false, false, false, 70),
            ("VTYT-DAY-TRUYEN-DICH-VO-TRUNG-009", "Dây truyền dịch vô trùng", "Bộ dây truyền dịch", "Nhiều nhà sản xuất", 12000m, 0m, "vat-tu-y-te", false, false, false, 75),
            ("VTYT-QUE-TEST-DUONG-HUYET-ACC-010", "Que test đường huyết Accu-Chek Instant", "Que thử dùng với máy tương thích", "Roche", 310000m, 0m, "vat-tu-y-te", false, false, false, 80),
        };

        var existing = await _db.Medications.AsNoTracking()
            .Where(m => seed.Select(s => s.Sku).Contains(m.Sku))
            .Select(m => m.Sku)
            .ToListAsync(ct);
        var existingSet = new HashSet<string>(existing, StringComparer.OrdinalIgnoreCase);

        var now = DateTime.UtcNow;
        var toAdd = new List<Medication>();
        foreach (var s in seed)
        {
            if (existingSet.Contains(s.Sku)) continue;
            if (!cats.ContainsKey(s.Slug)) continue;

            toAdd.Add(new Medication
            {
                Sku = s.Sku,
                Name = s.Name,
                GenericName = string.IsNullOrEmpty(s.Generic) ? null : s.Generic,
                Manufacturer = s.Mfr,
                Price = s.Price,
                DiscountPercent = s.Discount,
                CategoryId = Cat(s.Slug),
                IsFeatured = s.Featured,
                IsBestSeller = s.BestSeller,
                IsPrescriptionRequired = s.Rx,
                StockQuantity = s.Stock,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
            });
        }

        if (toAdd.Count > 0)
        {
            _db.Medications.AddRange(toAdd);
            await _db.SaveChangesAsync(ct);
        }
    }

    // -------------------------------------------------------------------------
    // Doctors
    // -------------------------------------------------------------------------
    private async Task SeedDoctorsAsync(CancellationToken ct)
    {
        if (await _db.Doctors.AnyAsync(ct)) return;

        var now = DateTime.UtcNow;
        _db.Doctors.AddRange(
            new Doctor { FullName = "BS Nguyen Van A", Specialization = "Noi tong quat", Hospital = "BV Bach Mai",      LicenseNumber = "BS-001", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Doctor { FullName = "BS Tran Thi B",   Specialization = "Nhi khoa",      Hospital = "BV Nhi TW",         LicenseNumber = "BS-002", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Doctor { FullName = "BS Le Van C",     Specialization = "Tim mach",      Hospital = "BV Tim Ha Noi",     LicenseNumber = "BS-003", IsActive = true, CreatedAt = now, UpdatedAt = now }
        );
        await _db.SaveChangesAsync(ct);
    }

    // -------------------------------------------------------------------------
    // Admin User
    // -------------------------------------------------------------------------
    private async Task SeedAdminUserAsync(CancellationToken ct)
    {
        if (await _db.Users.AnyAsync(u => u.Email == AdminEmail, ct)) return;

        var now = DateTime.UtcNow;
        var admin = new User
        {
            FullName = "PharmaIntel Admin",
            Email = AdminEmail,
            AuthProvider = "local",
            Role = "admin",
            IsTermsAccepted = true,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
        admin.PasswordHash = _hasher.HashPassword(admin, AdminPassword);

        _db.Users.Add(admin);
        await _db.SaveChangesAsync(ct);
    }

    // -------------------------------------------------------------------------
    // Demo User + Address + PaymentMethod
    // -------------------------------------------------------------------------
    private async Task SeedDemoUserAsync(CancellationToken ct)
    {
        if (await _db.Users.AnyAsync(u => u.Email == DemoEmail, ct)) return;

        var now = DateTime.UtcNow;
        var demo = new User
        {
            FullName = "Demo User",
            Email = DemoEmail,
            AuthProvider = "local",
            Role = "user",
            IsTermsAccepted = true,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
        demo.PasswordHash = _hasher.HashPassword(demo, DemoPassword);

        _db.Users.Add(demo);
        await _db.SaveChangesAsync(ct);

        _db.Addresses.Add(new Address
        {
            UserId = demo.Id,
            RecipientName = "Demo User",
            Phone = "0901234567",
            Province = "Ha Noi",
            District = "Cau Giay",
            Ward = "Dich Vong",
            StreetAddress = "So 1 Pham Van Bach",
            IsDefault = true,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        _db.PaymentMethods.Add(new PaymentMethod
        {
            UserId = demo.Id,
            PaymentType = "cod",
            DisplayName = "Thanh toan khi nhan hang (COD)",
            IsDefault = true,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        await _db.SaveChangesAsync(ct);
    }
}
