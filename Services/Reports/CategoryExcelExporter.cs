using System.ComponentModel;
using App.Services.Categories.Dto;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Hosting;

namespace App.Services.Reports
{
    public class CategoryExcelExporter : ICategoryExcelExporter
    {
        private readonly IWebHostEnvironment _env;

        public CategoryExcelExporter(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> ExportAsync(List<CategoryReportDto> data)
        {
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Kategoriler");

            worksheet.Cell(1, 1).Value = "Kod";
            worksheet.Cell(1, 2).Value = "Ad";
            worksheet.Cell(1, 3).Value = "Bağlı Olduğu Kategori";
            worksheet.Cell(1, 4).Value = "Alt Kategori Sayısı";

            var headerRange = worksheet.Range("A1:D1");
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            int row = 2;
            foreach (var cat in data)
            {
                worksheet.Cell(row, 1).Value = cat.CategoryCode;
                worksheet.Cell(row, 2).Value = cat.Name;
                worksheet.Cell(row, 3).Value = cat.ParentCategoryName ?? "Ana Kategori";
                worksheet.Cell(row, 4).Value = cat.SubCategoryCount;
                row++;
            }

            worksheet.Columns().AdjustToContents();

            var exportDir = Path.Combine(_env.WebRootPath ?? "wwwroot", "exports");
            if (!Directory.Exists(exportDir))
                Directory.CreateDirectory(exportDir);

            var fileName = $"categories_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            var filePath = Path.Combine(exportDir, fileName);
            workbook.SaveAs(filePath);

            return fileName;
        }

        public async Task<byte[]> FilteredColumnsExportAsync(List<CategoryReportDto> categories, List<string>? selectedColumns = null)
        {
            var type = typeof(CategoryReportDto);
            var allProps = type.GetProperties();

            // Kolonları belirle
            var columns = selectedColumns ?? allProps.Select(p => p.Name).ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Kategoriler");

            // Başlıklar
            for (int i = 0; i < columns.Count; i++)
            {
                var prop = allProps.FirstOrDefault(p => string.Equals(p.Name, columns[i], StringComparison.OrdinalIgnoreCase));
                var displayName = prop?.GetCustomAttributes(typeof(DisplayNameAttribute), false)
                    .Cast<DisplayNameAttribute>()
                    .FirstOrDefault()?.DisplayName ?? columns[i];

                worksheet.Cell(1, i + 1).Value = displayName;
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightSteelBlue;
                worksheet.Cell(1, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // İçerikler
            for (int row = 0; row < categories.Count; row++)
            {
                var category = categories[row];

                for (int col = 0; col < columns.Count; col++)
                {
                    var prop = allProps.FirstOrDefault(p => string.Equals(p.Name, columns[col], StringComparison.OrdinalIgnoreCase));
                    var value = prop?.GetValue(category);
                    worksheet.Cell(row + 2, col + 1).SetValue(value?.ToString() ?? "");
                }
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
