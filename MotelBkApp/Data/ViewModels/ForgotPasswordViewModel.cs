using System.ComponentModel.DataAnnotations;

namespace MotelBkApp.Data.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
