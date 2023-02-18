using Microsoft.AspNetCore.Identity;

namespace MotelBkApp.Models
{
    
        public class AppUser : IdentityUser<int>
        {
            public string FirstName { get; set; } = String.Empty;
            public string LastName { get; set; } = String.Empty;
            public DateOnly DOB { get; set; } = DateOnly.FromDateTime(DateTime.Now);
            public Motel? Motel { get; set; }

        }
    
}
