namespace App.Repositories.Products
{
    public class ProductImage : BaseEntity<int>
    {
        public int ProductId { get; set; }
        public string ImagePath { get; set; } = default!;
        public DateTime Created { get; set; }

        // 🔁 Navigation property (istersen include edersin)
        public Product Product { get; set; } = default!;
    }
}
