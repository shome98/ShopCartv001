using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Retail_MVC.Models
{
    public class Vendor
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set;}
        public string? PhoneNumber { get; set; }
    }
}
