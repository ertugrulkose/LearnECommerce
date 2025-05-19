using App.Repositories;
using App.Repositories.Products;
using App.Services.Categories.Search;
using App.Services.Products.Create;
using App.Services.Products.Update;
using App.Services.Products.UpdateStock;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace App.Services.Products
{
    public class ProductService(IProductRepository productRepository, IUnitOfWork unitOfWork,IMapper mapper): IProductService
    {
        public async Task<ServiceResult<List<ProductDto>>> GetTopPriceProductsAsync(int count)
        {
            var products = await productRepository.GetTopPriceProductsAsync(count);

            var productsAsDto = mapper.Map<List<ProductDto>>(products);

            return new ServiceResult<List<ProductDto>>()
            {
                Data = productsAsDto
            };
        }
        
        public async Task<ServiceResult<List<ProductDto>>> GetAllListAsync()
        {
            var products = await productRepository.GetAll()
                .Include(p => p.Category) // 🆕
                .ToListAsync();

            #region Manuel Mapping
            //var productsAsDto = products.Select(p => new ProductDto(p.Id, p.Name, p.Price, p.Stock)).ToList();
            #endregion

            var productsAsDto = mapper.Map<List<ProductDto>>(products);

            return ServiceResult<List<ProductDto>>.Success(productsAsDto);
        }
       
        public async Task<ServiceResult<PagedResult<ProductDto>>> GetPagedAllListAsync(int pageNumber, int pageSize)
        {
            var totalCount = await productRepository.GetAll().CountAsync();

            var products = await productRepository.GetAll()
                .Include(p => p.Category) // 🆕
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            #region Manuel Mapping
            //var productsAsDto = products.Select(p => new ProductDto(p.Id, p.Name, p.Price, p.Stock)).ToList();
            #endregion

            var productsAsDto = mapper.Map<List<ProductDto>>(products);

            // 🆕 PagedResult içerisine hem listeyi hem de totalCount'u koyuyoruz
            var pagedResult = new PagedResult<ProductDto>(productsAsDto, totalCount);

            return ServiceResult<PagedResult<ProductDto>>.Success(pagedResult);
        }

        public async Task<ServiceResult<ProductDto?>> GetByIdAsync(int id)
        {
            var product = await productRepository.GetAll()
                .Include(p => p.Category) // 🆕
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product is null)
            {
                return ServiceResult<ProductDto?>.Fail("Product not found", HttpStatusCode.NotFound);
            }

            var productsAsDto = mapper.Map<ProductDto>(product);

            return ServiceResult<ProductDto>.Success(productsAsDto)!;
        }
        
        public async Task<ServiceResult<CreateProductResponse>> CreateAsync(CreateProductRequest request)
        {
            // test CriticalException
            //throw new CriticalException("Kritik seviye bir hata oluştu");

            // async manuel service business check
            var anyProduct = await productRepository.Where(x => x.Name == request.Name).AnyAsync();

            if (anyProduct)
            {
                return ServiceResult<CreateProductResponse>.Fail("Ürün İsmi Veritabanında Bulunmaktadır.", HttpStatusCode.BadRequest);
            }

            var product = mapper.Map<Product>(request);

            await productRepository.AddAsync(product);
            await unitOfWork.SaveChangesAsync();
            return ServiceResult<CreateProductResponse>.SuccessAsCreated(new CreateProductResponse(product.Id),$"api/products/{product.Id}");
        }
        
        public async Task<ServiceResult> UpdateAsync(int id, UpdateProductRequest request)
        {
            var isProductNameExist = await productRepository.Where(x => x.Name == request.Name && x.Id != id).AnyAsync();

            if (isProductNameExist)
            {
                return ServiceResult.Fail("Ürün İsmi Veritabanında Bulunmaktadır.", HttpStatusCode.BadRequest);
            }

            var product = mapper.Map<Product>(request);
            product.Id = id;

            productRepository.Update(product);
            await unitOfWork.SaveChangesAsync();

            return ServiceResult.Success(HttpStatusCode.NoContent);
        }
        
        public async Task<ServiceResult> UpdateStockAsync(UpdateProductStockRequest request)
        {
            var product = await productRepository.GetByIdAsync(request.ProductId);

            if (product is null)
            {
                return ServiceResult.Fail("Product not found", HttpStatusCode.NotFound);
            }

            product.Stock = request.Quantity;

            productRepository.Update(product);
            await unitOfWork.SaveChangesAsync();

            return ServiceResult.Success(HttpStatusCode.NoContent);
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var product = await productRepository.GetByIdAsync(id);

            productRepository.Delete(product!);
            await unitOfWork.SaveChangesAsync();

            return ServiceResult.Success(HttpStatusCode.NoContent);
        }
    }
}
 