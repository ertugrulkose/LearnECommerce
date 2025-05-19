using App.Repositories.Categories;

namespace App.Repositories.Products
{
    public class Product:BaseEntity<int>, IAuditEntity
    {
        public string Name { get; set; } = default!;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int CategoryId { get; set; }
        public string? ThumbnailPath { get; set; }
        public Category Category { get; set; } = default!;
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }

        // 🔁 Çoklu görseller
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    }
}