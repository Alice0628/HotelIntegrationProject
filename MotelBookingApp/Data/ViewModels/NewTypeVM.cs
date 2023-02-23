using MotelBookingApp.Models;
using System.ComponentModel.DataAnnotations;

namespace MotelBookingApp.Data.ViewModels
{
    public class NewTypeVM
    {
        [Required]
        public int Id { get; set; } = default;

        [Display(Name = "Type Name")]
        [Required(ErrorMessage = "Type name is required")]
        public string Name { get; set; } = String.Empty;
        [Display(Name = "Price")]
        [Range(0.01, 100000000, ErrorMessage = "Price must be greter than zero !")]
        public decimal Price { get; set; } = default;

        [Required]
        [MaxLength(100, ErrorMessage ="the maximum length is 100")]
        public string Description { get; set; } = String.Empty;

       
        public IFormFile? Image { get; set; } = null;
      

        public string? ImageUrl { get; set; }
       
        [Display(Name = "Sleep Number")]
        [Required(ErrorMessage = "Sleep number is required")]
        [Range(1,10, ErrorMessage = "Sleep number must be greter than zero and less than 10 !")]
        public int Sleep { get; set; } = default;
        [Display(Name = "Total amount")]
        [Required(ErrorMessage = "Amount number is required")]
        [Range(0, 10000000000, ErrorMessage = "Amount can not be negative!")]
        public int Amount { get; set; } = default;
        [Required]
        public Motel Motel { get; set; } = new Motel();
    }
}
