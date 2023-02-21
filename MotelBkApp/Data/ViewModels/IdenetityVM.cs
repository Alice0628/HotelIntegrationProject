using System.ComponentModel.DataAnnotations;

namespace MotelBkApp.Data.ViewModels
{
    public class IdenetityVM
    {
        public int Id { get; set; } = default;

        [Required(ErrorMessage = "First name is required")]
        [RegularExpression(@"^[a-zA-Z ]{1,100}$", ErrorMessage = "Unsupported character.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [RegularExpression(@"^[a-zA-Z ]{1,100}$", ErrorMessage = "Unsupported character.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "DOB is required")]
        public DateOnly DOB { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        [Required(ErrorMessage = "User name is required")]
        [RegularExpression(@"^[a-zA-Z0-9-._@+]{1,100}$", ErrorMessage = "Unsupported character.")]
        public string UserName { get; set; } = String.Empty;

        public string Email { get; set; } = String.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Exactly 10 digit numbers allowed.")]
        public string PhoneNumber { get; set; } = String.Empty;
    }
}
