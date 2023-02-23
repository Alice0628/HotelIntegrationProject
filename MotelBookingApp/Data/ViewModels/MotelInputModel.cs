//using Amazon.DynamoDBv2.DataModel;

using System.ComponentModel.DataAnnotations;

namespace MotelBookingApp.Data.ViewModels
{
    public class MotelInputModel
    {

        public int Id { get; set; }
        [Display(Name = "Motel Name")]
        [Required(ErrorMessage = "Motel name is required")]
        public string Name { get; set; }

        [Display(Name = "Motel Address")]
        [Required(ErrorMessage = "Motel Address is required")]
        public string Address { get; set; }

        [Display(Name = "Motel City")]
        [Required(ErrorMessage = "Motel City is required")]
        public string City { get; set; }

        [Display(Name = "Motel Province")]
        [Required(ErrorMessage = "Motel Province is required")]
        public string Province { get; set; }

        [Display(Name = "Motel PostalCode")]
        [Required(ErrorMessage = "Motel PostalCode is required")]
        public string PostalCode { get; set; }

        [Display(Name = "Motel Image")]
        [Required(ErrorMessage = "Please upload motel image")]
        public IFormFile MotelImage { get; set; }

        public string? ImageUrl { get; set; }

        public double? Score { get; set; }
    }
}
