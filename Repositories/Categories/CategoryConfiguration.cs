using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Repositories.Categories
{
    public class CategoryConfiguration:IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(150);

            // 🔹 Kategori Kodu (Unique ve Zorunlu)
            builder.Property(x => x.CategoryCode)
                .IsRequired()
                .HasMaxLength(20); // Uzunluğu belirleyelim
            builder.HasIndex(x => x.CategoryCode).IsUnique(); // Unique olarak ayarladık

            // 🔹 Parent-Child Kategori İlişkisi (Self Referencing)
            builder.HasOne(x => x.ParentCategory)
                .WithMany(x => x.SubCategories)
                .HasForeignKey(x => x.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict); // Kategoriler silinirken çocukları etkilememesi için

            // 🔹 Ürünlerle İlişki
            builder.HasMany(x => x.Products)
                .WithOne(x => x.Category)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Cascade); // Kategori silinirse, ürünleri de silinsin
        }
    }
}
