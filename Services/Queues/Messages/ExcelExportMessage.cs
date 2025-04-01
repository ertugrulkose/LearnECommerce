namespace App.Services.Queues.Messages
{
    public class ExcelExportMessage
    {
        public string ExportType { get; set; } = default!;     // Örn: "category", "product", "user"
        public string? RequestedBy { get; set; }               // E-postası, kullanıcı adı vs.
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow; // Default veriyoruz
        public Dictionary<string, string>? Filters { get; set; } // İsteğe bağlı filtre parametreleri
        public List<string>? Columns { get; set; } // İstenen kolonlar
        public SortConfig? Sort { get; set; }
    }
    public class SortConfig
    {
        public string Key { get; set; } = string.Empty;
        public string Direction { get; set; } = "asc";
    }

}
