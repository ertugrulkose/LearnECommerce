namespace App.Services.Categories.Dto;

    public record CategoryWithSubcategoriesDto(int Id, string Name, string CategoryCode, IReadOnlyList<CategoryDto> SubCategories);

