using App.Services.Categories;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Hosting;

namespace App.Services.Exporters
{
    public class CategoryExcelExporter
    {
        private readonly ICategoryService _categoryService;
        private readonly IWebHostEnvironment _env;

        public CategoryExcelExporter(ICategoryService categoryService, IWebHostEnvironment env)
        {
            _categoryService = categoryService;
            _env = env;
        }

        public async Task<string> ExportAsync()
        {
            var categoriesResult = await _categoryService.GetAllListAsync();

            if (!categoriesResult.IsSuccess || categoriesResult.Data == null)
                throw new Exception("Kategoriler alınamadı!");

            var categories = categoriesResult.Data; // ✅ Listeye ulaştık

            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Kategoriler");

            // Başlık satırını yaz
            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Kod";
            worksheet.Cell(1, 3).Value = "Ad";
            worksheet.Cell(1, 4).Value = "Bağlı Olduğu Kategori";

            int row = 2;
            foreach (var cat in categories)
            {
                worksheet.Cell(row, 1).Value = cat.Id;
                worksheet.Cell(row, 2).Value = cat.CategoryCode;
                worksheet.Cell(row, 3).Value = cat.Name;
                worksheet.Cell(row, 4).Value = cat.ParentCategoryId == null
                    ? "Ana Kategori"
                    : categories.FirstOrDefault(c => c.Id == cat.ParentCategoryId)?.Name ?? "Bilinmiyor";
                row++;
            }

            // Klasörü oluştur
            var exportDir = Path.Combine(_env.WebRootPath ?? "wwwroot", "exports");
            if (!Directory.Exists(exportDir))
                Directory.CreateDirectory(exportDir);

            var fileName = $"categories_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            var filePath = Path.Combine(exportDir, fileName);

            workbook.SaveAs(filePath);

            return fileName; // ister path, ister sadece dosya adı dön
        }
    }
}
