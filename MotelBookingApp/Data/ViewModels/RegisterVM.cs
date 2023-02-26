using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using MotelBookingApp.Models;

namespace MotelBookingApp.Data.ViewModels
{
    public class RegisterVM
    {
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
        [DataType(DataType.Date)]
        public DateTime DOB { get; set; } = DateTime.Now;

        [Display(Name = "Email address")]
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Phone number")]
        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Exactly 10 digit numbers allowed.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Password")]
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Confirm password")]
        [Required(ErrorMessage = "Confirm password is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public Motel? Motel { get; set; } = new Motel();
    }
}
