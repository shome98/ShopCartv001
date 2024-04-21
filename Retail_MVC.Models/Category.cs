using System.ComponentModel.DataAnnotations;

namespace Retail_MVC.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string? Name { get; set; }
        public int? DisplayOrder { get; set; }
    }
}
