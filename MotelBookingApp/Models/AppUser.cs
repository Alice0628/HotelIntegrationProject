using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MotelBookingApp.Models
{
    public class AppUser : IdentityUser<int>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime DOB { get; set; } = DateTime.Now;

        public Motel? Motel { get; set; }  
    }
}
