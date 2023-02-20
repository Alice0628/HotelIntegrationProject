using System.ComponentModel.DataAnnotations;
using MotelBkApp.Models;

namespace MotelBkApp.Data.ViewModels
{
    public class EditStaffVM
    {
        public int Id { get; set; }

        [Display(Name = "User Name")]
        [Required(ErrorMessage = "User name is required")]
        public string UserName { get; set; } = string.Empty;

        [Display(Name = "First Name")]
        [Required(ErrorMessage = "First name is required")]
        [RegularExpression(@"^[a-zA-Z ]{1,100}$", ErrorMessage = "Unsupported character.")]
        public string FirstName { get; set; } = string.Empty;

        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "Last name is required")]
        [RegularExpression(@"^[a-zA-Z ]{1,100}$", ErrorMessage = "Unsupported character.")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Date of Birth")]
        [Required(ErrorMessage = "DOB is required")]
        public DateOnly DOB { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        [Display(Name = "Email address")]
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Phone number")]
        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Exactly 10 digit numbers allowed.")]
        public string PhoneNumber { get; set; } = string.Empty;

       
        public int Motel { get; set; }

        public List<Motel>? MotelList { get; set; }

    }
}
