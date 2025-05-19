namespace App.Services.Products
{
    public record ProductDetailDto(
        int Id,
        string Name,
        decimal Price,
        int Stock,
        int CategoryId,
        string CategoryName,
        string? ThumbnailPath,
        List<string>? ImagePaths
    );
}
