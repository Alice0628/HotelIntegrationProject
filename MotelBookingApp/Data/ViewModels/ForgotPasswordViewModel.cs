using System.ComponentModel.DataAnnotations;

namespace MotelBookingApp.Data.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
