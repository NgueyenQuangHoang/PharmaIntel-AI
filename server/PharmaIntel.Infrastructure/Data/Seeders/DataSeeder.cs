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
        await SeedDoctorsAsync(ct);
        await SeedAdminUserAsync(ct);
        await SeedDemoUserAsync(ct);
    }

    // -------------------------------------------------------------------------
    // Symptoms
    // -------------------------------------------------------------------------
    private async Task SeedSymptomsAsync(CancellationToken ct)
    {
        if (await _db.Symptoms.AnyAsync(ct)) return;

        var items = new[]
        {
            new Symptom { Name = "Sot",        GroupName = "toan than", DisplayOrder = 1 },
            new Symptom { Name = "Nhuc moi",   GroupName = "toan than", DisplayOrder = 2 },
            new Symptom { Name = "Chong mat",  GroupName = "toan than", DisplayOrder = 3 },
            new Symptom { Name = "Ho",         GroupName = "ho hap",    DisplayOrder = 1 },
            new Symptom { Name = "So mui",     GroupName = "ho hap",    DisplayOrder = 2 },
            new Symptom { Name = "Dau hong",   GroupName = "ho hap",    DisplayOrder = 3 },
            new Symptom { Name = "Kho tho",    GroupName = "ho hap",    DisplayOrder = 4 },
            new Symptom { Name = "Dau dau",    GroupName = "than kinh", DisplayOrder = 1 },
            new Symptom { Name = "Buon non",   GroupName = "tieu hoa",  DisplayOrder = 1 },
            new Symptom { Name = "Dau bung",   GroupName = "tieu hoa",  DisplayOrder = 2 },
            new Symptom { Name = "Tieu chay",  GroupName = "tieu hoa",  DisplayOrder = 3 },
            new Symptom { Name = "Dau nguc",   GroupName = "tim mach",  DisplayOrder = 1 }
        };

        _db.Symptoms.AddRange(items);
        await _db.SaveChangesAsync(ct);
    }

    // -------------------------------------------------------------------------
    // Categories
    // -------------------------------------------------------------------------
    private async Task SeedCategoriesAsync(CancellationToken ct)
    {
        if (await _db.Categories.AnyAsync(ct)) return;

        var now = DateTime.UtcNow;
        var names = new[]
        {
            "Cam cum",
            "Tieu hoa",
            "Dau va ha sot",
            "Vitamin va khoang chat",
            "Da lieu",
            "Phu nu va tre em"
        };

        var order = 1;
        foreach (var name in names)
        {
            _db.Categories.Add(new Category
            {
                Name = name,
                Slug = SlugHelper.ToSlug(name),
                DisplayOrder = order++,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
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

        var painSlug    = SlugHelper.ToSlug("Dau va ha sot");
        var coldSlug    = SlugHelper.ToSlug("Cam cum");
        var digestSlug  = SlugHelper.ToSlug("Tieu hoa");
        var vitaminSlug = SlugHelper.ToSlug("Vitamin va khoang chat");

        var now = DateTime.UtcNow;
        var meds = new[]
        {
            new Medication { Sku = "MED-PARA-500", Name = "Paracetamol 500mg",   GenericName = "Paracetamol", CategoryId = Cat(painSlug),
                Manufacturer = "Stada", Price = 15000m, DiscountPercent = 5m, StockQuantity = 500, Packaging = "Hop 10 vi x 10 vien",
                ActiveIngredients = "Paracetamol 500mg", UsageInstructions = "Uong 1-2 vien moi 4-6 gio khi sot/dau, khong qua 8 vien/ngay.",
                IsFeatured = true, IsBestSeller = true, IsPrescriptionRequired = false, IsActive = true, CreatedAt = now, UpdatedAt = now },

            new Medication { Sku = "MED-IBU-400",  Name = "Ibuprofen 400mg",      GenericName = "Ibuprofen", CategoryId = Cat(painSlug),
                Manufacturer = "Hau Giang", Price = 25000m, DiscountPercent = 0m, StockQuantity = 300, Packaging = "Hop 3 vi x 10 vien",
                ActiveIngredients = "Ibuprofen 400mg", UsageInstructions = "Uong 1 vien moi 6-8 gio sau an.",
                IsBestSeller = true, IsPrescriptionRequired = false, IsActive = true, CreatedAt = now, UpdatedAt = now },

            new Medication { Sku = "MED-DECOL",    Name = "Decolgen Forte",       CategoryId = Cat(coldSlug),
                Manufacturer = "United Pharma", Price = 32000m, DiscountPercent = 10m, StockQuantity = 250, Packaging = "Hop 25 vi x 4 vien",
                ActiveIngredients = "Paracetamol + Phenylephrin + Chlorpheniramin", UsageInstructions = "Uong 1 vien khi co trieu chung cam cum.",
                IsFeatured = true, IsPrescriptionRequired = false, IsActive = true, CreatedAt = now, UpdatedAt = now },

            new Medication { Sku = "MED-STREP",    Name = "Strepsils huong mat",  CategoryId = Cat(coldSlug),
                Manufacturer = "Reckitt", Price = 45000m, DiscountPercent = 0m, StockQuantity = 200, Packaging = "Hop 24 vien ngam",
                UsageInstructions = "Ngam 1 vien moi 2-3 gio khi dau hong.",
                IsPrescriptionRequired = false, IsActive = true, CreatedAt = now, UpdatedAt = now },

            new Medication { Sku = "MED-LORAT",    Name = "Loratadine 10mg",      CategoryId = Cat(coldSlug),
                Manufacturer = "DHG", Price = 28000m, DiscountPercent = 0m, StockQuantity = 180, Packaging = "Hop 1 vi x 10 vien",
                ActiveIngredients = "Loratadine 10mg", UsageInstructions = "Uong 1 vien moi ngay khi di ung / so mui.",
                IsPrescriptionRequired = false, IsActive = true, CreatedAt = now, UpdatedAt = now },

            new Medication { Sku = "MED-SMEC",     Name = "Smecta",               CategoryId = Cat(digestSlug),
                Manufacturer = "Ipsen", Price = 65000m, DiscountPercent = 5m, StockQuantity = 150, Packaging = "Hop 30 goi bot",
                ActiveIngredients = "Diosmectite", UsageInstructions = "Pha 1 goi voi 50ml nuoc, uong sau bua an.",
                IsBestSeller = true, IsPrescriptionRequired = false, IsActive = true, CreatedAt = now, UpdatedAt = now },

            new Medication { Sku = "MED-BERB",     Name = "Berberin 50mg",        CategoryId = Cat(digestSlug),
                Manufacturer = "Mekophar", Price = 18000m, DiscountPercent = 0m, StockQuantity = 220, Packaging = "Lo 100 vien",
                UsageInstructions = "Uong 4-6 vien/ngay khi tieu chay nhe.",
                IsPrescriptionRequired = false, IsActive = true, CreatedAt = now, UpdatedAt = now },

            new Medication { Sku = "MED-DOMP",     Name = "Domperidon 10mg",      CategoryId = Cat(digestSlug),
                Manufacturer = "Sanofi", Price = 22000m, DiscountPercent = 0m, StockQuantity = 140, Packaging = "Hop 3 vi x 10 vien",
                ActiveIngredients = "Domperidon", UsageInstructions = "Uong 1 vien truoc bua an 15-30 phut khi buon non.",
                IsPrescriptionRequired = false, IsActive = true, CreatedAt = now, UpdatedAt = now },

            new Medication { Sku = "MED-ENTERO",   Name = "Enterogermina",        CategoryId = Cat(digestSlug),
                Manufacturer = "Sanofi", Price = 95000m, DiscountPercent = 8m, StockQuantity = 160, Packaging = "Hop 20 ong x 5ml",
                UsageInstructions = "Uong 1-2 ong/ngay de ho tro he tieu hoa.",
                IsFeatured = true, IsPrescriptionRequired = false, IsActive = true, CreatedAt = now, UpdatedAt = now },

            new Medication { Sku = "MED-ORE",      Name = "Oresol bu dien giai",  CategoryId = Cat(digestSlug),
                Manufacturer = "Bidipharm", Price = 8000m, DiscountPercent = 0m, StockQuantity = 400, Packaging = "Goi 27.9g pha 1 lit",
                UsageInstructions = "Pha 1 goi voi 1 lit nuoc dun soi de nguoi, uong dan trong 24h.",
                IsPrescriptionRequired = false, IsActive = true, CreatedAt = now, UpdatedAt = now },

            new Medication { Sku = "MED-VITC",     Name = "Vitamin C 1000mg",     CategoryId = Cat(vitaminSlug),
                Manufacturer = "Bayer", Price = 120000m, DiscountPercent = 12m, StockQuantity = 280, Packaging = "Tup 20 vien sui",
                UsageInstructions = "Hoa tan 1 vien sui voi 200ml nuoc, uong moi sang.",
                IsFeatured = true, IsBestSeller = true, IsPrescriptionRequired = false, IsActive = true, CreatedAt = now, UpdatedAt = now },

            new Medication { Sku = "MED-ZINC",     Name = "Kem gluconate 70mg",   CategoryId = Cat(vitaminSlug),
                Manufacturer = "Mediplantex", Price = 55000m, DiscountPercent = 0m, StockQuantity = 130, Packaging = "Hop 30 vien",
                UsageInstructions = "Uong 1 vien/ngay sau bua an.",
                IsPrescriptionRequired = false, IsActive = true, CreatedAt = now, UpdatedAt = now }
        };

        _db.Medications.AddRange(meds);
        await _db.SaveChangesAsync(ct);
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
