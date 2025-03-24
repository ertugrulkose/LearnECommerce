using FluentValidation;

namespace App.Services.Products.Update
{
    public class UpdateProductRequestValidator:AbstractValidator<UpdateProductRequest>
    {
        public UpdateProductRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Ürün İsmi Gereklidir.")
                .Length(2, 100).WithMessage("Ürün İsmi 2-100 Karakter Arasında Olmalıdır.");

            // price validation
            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Fiyat 0'dan Büyük Olmalıdır.");

            // category validation
            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Ürün Kategori Değeri 0'dan Büyük Olmalıdır.");

            // stock InclusiveBetween validation
            RuleFor(x => x.Stock)
                .InclusiveBetween(1, 1000).WithMessage("Stok 1 ile 1000 Arasında Olmalıdır.");
        }
    }
}
