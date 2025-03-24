using App.Repositories.Products;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace App.Services.Products.Create
{
    public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
    {
        private readonly IProductRepository _productRepository;
        public CreateProductRequestValidator(IProductRepository productRepository)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Ürün İsmi Gereklidir.")
                .Length(2, 100).WithMessage("Ürün İsmi 2-100 Karakter Arasında Olmalıdır.");
            //.MustAsync(MustUniqueProductNameAsync).WithMessage("Ürün İsmi Veritabanında Bulunmaktadır.");
            //.Must(MustUniqueProductName).WithMessage("Ürün İsmi Veritabanında Bulunmaktadır.");


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

        #region 2. Way async validation

        //private async Task<bool> MustUniqueProductNameAsync(string name, CancellationToken cancellationToken) 
        //{
        //    // async validation için ServiceExtension.cs içerisinde AddFluentValidationAutoValidation() metodu kapatılmalıdır.
        //    return !await _productRepository.Where(x => x.Name == name).AnyAsync(cancellationToken);
        //}
        #endregion

        #region 1. Way sync validation

        //private bool MustUniqueProductName(string name)
        //{
        //    return !_productRepository.Where(x => x.Name == name).Any();
        //    // false => bir hata var
        //    // true => bir hata yok
        //}
        #endregion
    }
}
