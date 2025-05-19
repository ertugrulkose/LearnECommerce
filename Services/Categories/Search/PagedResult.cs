namespace App.Services.Categories.Search
{
    public record PagedResult<T>(List<T> Items, int TotalCount);
}
