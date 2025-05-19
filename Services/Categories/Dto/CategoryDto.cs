namespace App.Services.Categories.Dto;

public record CategoryDto(int Id,
    string Name,
    string CategoryCode,
    int? ParentCategoryId,
    string? ParentCategoryName,
    List<SubCategoryDto> SubCategories // 🆕 Eklendi!
    );
    public record SubCategoryDto(
        int Id,
        string Name
        );

