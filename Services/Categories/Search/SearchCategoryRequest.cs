using App.Services.Queues.Messages;

namespace App.Services.Categories.Search
{
    public class SearchCategoryRequest
    {
        public Dictionary<string, string>? Filters { get; set; }
        public SortConfig? Sort { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
