using Microsoft.AspNetCore.Identity;

namespace MotelBookingApp.Models
{
    public class AppUser : IdentityUser<int>
    {
        public string FirstName { get; set; } = String.Empty;
        public string LastName { get; set; } = String.Empty;
        public DateTime DOB { get; set; }
        public Motel? Motel { get; set; } = new Motel();

    }   

}
