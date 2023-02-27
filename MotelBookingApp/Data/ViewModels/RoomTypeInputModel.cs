using MotelBookingApp.Models;
using System.ComponentModel.DataAnnotations;

namespace MotelBookingApp.Data.ViewModels
{
    public class RoomTypeInputModel
    {
        [Required]
        public int Id { get; set; } = default;

        [Display(Name = "Type Name")]
        [Required(ErrorMessage = "Type name is required")]
        public string Name { get; set; } = String.Empty;


        [Required]
        [MaxLength(100, ErrorMessage = "the maximum length is 100")]
        public string Description { get; set; } = String.Empty;


        public IFormFile? TypeImage { get; set; }

        public string? ImageUrl { get; set; }

        [Display(Name = "Sleep Number")]
        [Required(ErrorMessage = "Sleep number is required")]
        [Range(1, 10, ErrorMessage = "Sleep number must be greter than zero and less than 10 !")]
        public int Sleep { get; set; } = default;

    }
}
