using Microsoft.AspNetCore.Identity;

namespace MotelBookingApp.Models
{
    public class AppUser : IdentityUser<int>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateOnly DOB { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public Motel? Motel { get; set; }
    }
}
