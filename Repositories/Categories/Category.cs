using App.Repositories.Products;
using System.Globalization;
using System.Text;

namespace App.Repositories.Categories
{
    public class Category : BaseEntity<int>, IAuditEntity
    {
        public string Name { get; set; } = default!;

        // 🔹 Yeni eklenen alan: Kategori Kodu (Otomatik atanacak ve unique olacak)
        public string CategoryCode { get; private set; } = default!;

        // 🔹 Alt Kategori yapısını desteklemek için Parent ID
        public int? ParentCategoryId { get; set; }
        public Category? ParentCategory { get; set; }
        public List<Category>? SubCategories { get; set; }

        public List<Product>? Products { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }

        // Constructor içinde kod ataması yapılır
        public Category(string name, int? parentCategoryId = null)
        {
            Name = name;
            CategoryCode = GenerateCategoryCode(name);
            ParentCategoryId = parentCategoryId;
        }

        // 🔹 Kategori kodunu otomatik üreten metot
        private static string GenerateCategoryCode(string name)
        {
            var normalized = name.Normalize(NormalizationForm.FormD)
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) // Türkçe karakterleri temizler
                .Aggregate("", (current, c) => current + c)
                .ToUpper(CultureInfo.InvariantCulture) // Kültür bağımsız olarak büyüt
                .Replace("İ", "I") // Özel Türkçe karakterleri düzelt
                .Replace("Ğ", "G")
                .Replace("Ü", "U")
                .Replace("Ş", "S")
                .Replace("Ö", "O")
                .Replace("Ç", "C");

            return normalized.Length >= 4
                ? normalized[..4] + "-" + Guid.NewGuid().ToString("N")[..4].ToUpper()
                : normalized + "-" + Guid.NewGuid().ToString("N")[..4].ToUpper();
        }
    }
}
