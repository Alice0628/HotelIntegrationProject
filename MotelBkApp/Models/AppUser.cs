using Microsoft.AspNetCore.Identity;

namespace MotelBkApp.Models
{
    
        public class AppUser : IdentityUser<int>
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }

            public DateOnly DOB { get; set; }

            public Motel? Motel { get; set; }


        }
    
}
