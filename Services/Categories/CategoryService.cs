using App.Repositories;
using App.Repositories.Categories;
using App.Services.Categories.Create;
using App.Services.Categories.Dto;
using App.Services.Categories.Search;
using App.Services.Categories.Update;
using App.Services.Queues.Messages;
using App.Services.Reports;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace App.Services.Categories
{
    public class CategoryService(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICategoryExcelExporter excelExporter) : ICategoryService
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
            var categories = await categoryRepository.GetAll()
                .Include(x => x.SubCategories)
                .Include(x => x.ParentCategory)
                .ToListAsync();
            var categoriesAsDto = mapper.Map<List<CategoryDto>>(categories);
            return ServiceResult<List<CategoryDto>>.Success(categoriesAsDto);
        }

        public async Task<ServiceResult<List<CategoryDto>>> GetPagedAllListAsync(int pageNumber, int pageSize)
        {
            var categories = await categoryRepository
                .GetAll()
                .Include(x => x.SubCategories)
                .Include(x => x.ParentCategory)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var categoriesDto = mapper.Map<List<CategoryDto>>(categories);

            return ServiceResult<List<CategoryDto>>.Success(categoriesDto);
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
                return ServiceResult<CategoryWithSubcategoriesDto>.Fail("Alt Kategori bulunamadı.",
                    HttpStatusCode.NotFound);
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
                return ServiceResult<int>.Fail("Bağlı olduğu kategori geçersiz bir değere sahip.",
                    HttpStatusCode.BadRequest);
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
            var isCategoryNameExist =
                await categoryRepository.Where(x => x.Name == request.Name && x.Id != id).AnyAsync();

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

        public async Task<string> ReportCategoriesToExcelAsync()
        {
            var categories = await categoryRepository.GetAll().ToListAsync();

            var reportData = categories.Select(cat => new CategoryReportDto
            {
                CategoryCode = cat.CategoryCode,
                Name = cat.Name,
                ParentCategoryName = cat.ParentCategoryId == null
                    ? "Ana Kategori"
                    : categories.FirstOrDefault(c => c.Id == cat.ParentCategoryId)?.Name,
                SubCategoryCount = categories.Count(c => c.ParentCategoryId == cat.Id)
            }).ToList();

            return await excelExporter.ExportAsync(reportData);
        }

        public async Task<byte[]> ReportCategoriesToExcelAsync(
            Dictionary<string, string>? filters,
            List<string>? columns,
            SortConfig? sortConfig)
        {
            var query = categoryRepository.GetAll();

            // 📌 SQL destekli filtreler
            if (filters != null)
            {
                if (filters.TryGetValue("name", out var nameFilter) && !string.IsNullOrWhiteSpace(nameFilter))
                    query = query.Where(x => x.Name.Contains(nameFilter));

                if (filters.TryGetValue("categoryCode", out var codeFilter) && !string.IsNullOrWhiteSpace(codeFilter))
                    query = query.Where(x => x.CategoryCode.Contains(codeFilter));

                if (filters.TryGetValue("parentCategoryId", out var parentIdFilter))
                {
                    if (parentIdFilter == "null")
                        query = query.Where(x => x.ParentCategoryId == null);
                    else if (int.TryParse(parentIdFilter, out var pid))
                        query = query.Where(x => x.ParentCategoryId == pid);
                }
            }

            var filtered = await query.ToListAsync();
            var allCategories = await categoryRepository.GetAll().ToListAsync();

            // 📌 Dto mapleme
            var reportData = filtered.Select(cat => new CategoryReportDto
            {
                CategoryCode = cat.CategoryCode,
                Name = cat.Name,
                ParentCategoryName = cat.ParentCategoryId == null
                    ? "Ana Kategori"
                    : allCategories.FirstOrDefault(c => c.Id == cat.ParentCategoryId)?.Name ?? "Bilinmiyor",
                SubCategoryCount = allCategories.Count(c => c.ParentCategoryId == cat.Id)
            }).ToList();

            // 📌 SubCategoryCount filtresi
            if (filters != null && filters.TryGetValue("subCategoryCount", out var subCountFilter))
            {
                if (subCountFilter == "0")
                    reportData = reportData.Where(x => x.SubCategoryCount == 0).ToList();
                else if (subCountFilter == "1+")
                    reportData = reportData.Where(x => x.SubCategoryCount > 0).ToList();
            }

            // 📌 Sıralama
            if (sortConfig is { Key: not null, Direction: not null })
            {
                var prop = typeof(CategoryReportDto).GetProperty(
                    sortConfig.Key,
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.IgnoreCase // 👈 bu satır çok önemli!
                );

                if (prop != null)
                {
                    reportData = sortConfig.Direction.ToLower() switch
                    {
                        "desc" => reportData.OrderByDescending(x => prop.GetValue(x)).ToList(),
                        _ => reportData.OrderBy(x => prop.GetValue(x)).ToList()
                    };
                }
            }


            return await excelExporter.FilteredColumnsExportAsync(reportData, columns);
        }

        public async Task<ServiceResult<PagedResult<CategoryDto>>> SearchCategoriesAsync(SearchCategoryRequest request)
        {
            var query = categoryRepository.GetAll();

            if (request.Filters?.TryGetValue("name", out var name) == true && !string.IsNullOrWhiteSpace(name))
                query = query.Where(x => x.Name.Contains(name));

            if (request.Filters?.TryGetValue("categoryCode", out var code) == true && !string.IsNullOrWhiteSpace(code))
                query = query.Where(x => x.CategoryCode.Contains(code));

            if (request.Filters?.TryGetValue("parentCategoryId", out var parentIdStr) == true && !string.IsNullOrWhiteSpace(parentIdStr))
            {
                if (parentIdStr == "null")
                {
                    // Ana kategoriler (Parent'ı olmayanlar)
                    query = query.Where(x => x.ParentCategoryId == null);
                }
                else if (int.TryParse(parentIdStr, out var parentId))
                {
                    query = query.Where(x => x.ParentCategoryId == parentId);
                }
            }

            if (request.Filters?.TryGetValue("subCategoryCount", out var subCategoryCount) == true && !string.IsNullOrWhiteSpace(subCategoryCount))
            {
                if (subCategoryCount == "0")
                {
                    // Alt kategorisi olmayanlar (SubCategories.Count == 0)
                    query = query.Where(x => x.SubCategories == null || !x.SubCategories.Any());
                }
                else if (subCategoryCount == "1+")
                {
                    // Alt kategorisi olanlar (SubCategories.Count > 0)
                    query = query.Where(x => x.SubCategories != null && x.SubCategories.Any());
                }
            }


            // 🆕 Parent ve SubCategories dahil ediyoruz
            query = query
                .Include(x => x.ParentCategory)
                .Include(x => x.SubCategories);

            // Total Count
            var totalCount = await query.CountAsync();

            // Pagination uygulanmış Items çekilir
            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();


            // DTO Mapping (AutoMapper zaten ParentCategoryName ve SubCategories dolduracak)
            var mappedItems = mapper.Map<List<CategoryDto>>(items);

            var result = new PagedResult<CategoryDto>(
                Items: mappedItems,
                TotalCount: totalCount
            );

            return ServiceResult<PagedResult<CategoryDto>>.Success(result);
        }
    }
}
