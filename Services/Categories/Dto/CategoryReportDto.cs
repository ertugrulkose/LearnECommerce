using System.ComponentModel;

namespace App.Services.Categories.Dto
{
    public class CategoryReportDto
    {
        [DisplayName("Kategori Kodu")]
        public string CategoryCode { get; set; }

        [DisplayName("Kategori Adı")]
        public string Name { get; set; }

        [DisplayName("Bağlı Olduğu Kategori")]
        public string ParentCategoryName { get; set; }

        [DisplayName("Alt Kategori Sayısı")]
        public int SubCategoryCount { get; set; }
    }
}
