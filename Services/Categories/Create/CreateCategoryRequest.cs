namespace App.Services.Categories.Create;

public record CreateCategoryRequest(string Name, int? ParentCategoryId);