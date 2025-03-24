using App.Repositories;
using App.Repositories.Categories;
using App.Services.Categories.Create;
using App.Services.Categories.Dto;
using App.Services.Categories.Update;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace App.Services.Categories
{
    public class CategoryService(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork, IMapper mapper) : ICategoryService
    {
        public async Task<ServiceResult<CategoryWithProductsDto>> GetCategoryWithProductsAsync(int categoryId)
        {
            var category = await categoryRepository.GetCategoryWithProductsAsync(categoryId);

            if (category is null)
            {
                return ServiceResult<CategoryWithProductsDto>.Fail("Kategori Bulunamadı", HttpStatusCode.NotFound);
            }
            var categoryAsDto = mapper.Map<CategoryWithProductsDto>(category);

            return ServiceResult<CategoryWithProductsDto>.Success(categoryAsDto);
        }

        public async Task<ServiceResult<List<CategoryWithProductsDto>>> GetCategoryByProductsAsync()
        {
            var categories = await categoryRepository.GetCategoryWithProducts().ToListAsync();

            var categoriesAsDto = mapper.Map<List<CategoryWithProductsDto>>(categories);

            return ServiceResult<List<CategoryWithProductsDto>>.Success(categoriesAsDto);
        }

        public async Task<ServiceResult<List<CategoryDto>>> GetAllListAsync()
        {
            var categories = await categoryRepository.GetAll().ToListAsync();
            var categoriesAsDto = mapper.Map<List<CategoryDto>>(categories);
            return ServiceResult<List<CategoryDto>>.Success(categoriesAsDto);
        }

        public async Task<ServiceResult<CategoryDto>> GetByIdAsync(int id)
        {
            var category = await categoryRepository.GetByIdAsync(id);

            if (category is null)
            {
                return ServiceResult<CategoryDto>.Fail("Kategori Bulunamadı", HttpStatusCode.NotFound);
            }

            var categoryAsDto = mapper.Map<CategoryDto>(category);

            return ServiceResult<CategoryDto>.Success(categoryAsDto);
        }

        public async Task<ServiceResult<CategoryWithSubcategoriesDto>> GetCategoryWithSubcategoriesAsync(int id)
        {
            var category = await categoryRepository.GetByIdWithSubcategoriesAsync(id);
            if (category is null)
            {
                return ServiceResult<CategoryWithSubcategoriesDto>.Fail("Alt Kategori bulunamadı.",HttpStatusCode.NotFound);
            }

            var categoryAsDto = mapper.Map<CategoryWithSubcategoriesDto>(category);

            return ServiceResult<CategoryWithSubcategoriesDto>.Success(categoryAsDto);
        }

        public async Task<ServiceResult<int>> CreateAsync(CreateCategoryRequest request)
        {
            // Aynı isimde bir kategori var mı kontrol et
            var anyCategory = await categoryRepository.Where(x => x.Name == request.Name).AnyAsync();

            if (anyCategory)
            {
                return ServiceResult<int>.Fail("Kategori İsmi Veritabanında Bulunmaktadır.", HttpStatusCode.NotFound);
            }

            // Geçersiz ParentCategoryId değerleri için kontrol (Negatif değer kabul edilmez)
            if (request.ParentCategoryId < 0)
            {
                return ServiceResult<int>.Fail("Bağlı olduğu kategori geçersiz bir değere sahip.", HttpStatusCode.BadRequest);
            }

            // Eğer ParentCategoryId 0 olarak geldiyse bunu null olarak kabul et (Ana Kategori)
            int? parentCategoryId = request.ParentCategoryId > 0 ? request.ParentCategoryId : null;

            // Eğer ParentCategoryId varsa geçerli olup olmadığı kontrol edilir
            if (parentCategoryId.HasValue)
            {
                var parentCategoryExists = await categoryRepository.Where(x => x.Id == parentCategoryId).AnyAsync();
                if (!parentCategoryExists)
                {
                    return ServiceResult<int>.Fail("Bağlı olduğu kategori bulunamadı.", HttpStatusCode.NotFound);
                }
            }

            // Yeni kategori nesnesi oluştur
            var newCategory = new Category(request.Name, parentCategoryId);

            // Mapleme yapılan hali. Mapleme yapmadık çünkü parentCategoryId için özel işlemler uyguladık
            //var newCategory = mapper.Map<Category>(request);
            //newCategory.ParentCategoryId = parentCategoryId;

            categoryRepository.AddAsync(newCategory);
            await unitOfWork.SaveChangesAsync();

            return ServiceResult<int>.SuccessAsCreated(newCategory.Id, $"api/categories/{newCategory.Id}");
        }

        public async Task<ServiceResult> UpdateAsync(int id, UpdateCategoryRequest request)
        {
            var isCategoryNameExist = await categoryRepository.Where(x => x.Name == request.Name && x.Id != id).AnyAsync();

            if (isCategoryNameExist)
            {
                return ServiceResult.Fail("Kategori İsmi Veritabanında Bulunmaktadır.", HttpStatusCode.BadRequest);
            }

            var category = mapper.Map<Category>(request);
            category.Id = id;

            categoryRepository.Update(category);
            await unitOfWork.SaveChangesAsync();

            return ServiceResult.Success(HttpStatusCode.NoContent);
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var category = await categoryRepository.GetByIdAsync(id);

            categoryRepository.Delete(category);
            await unitOfWork.SaveChangesAsync();
            return ServiceResult.Success(HttpStatusCode.NoContent);
        }
       
    }
}
