using App.Services.Categories.Dto;

namespace App.Services.Reports
{
    public interface ICategoryExcelExporter
    {
        Task<string> ExportAsync(List<CategoryReportDto> data);
        Task<byte[]> FilteredColumnsExportAsync(List<CategoryReportDto> categories, List<string>? selectedColumns = null);

    }
}
