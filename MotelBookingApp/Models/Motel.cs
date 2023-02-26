
namespace MotelBookingApp.Models
{

    public class Motel
    {

        public int Id { get; set; } = default;

        public string Name { get; set; } = String.Empty;

        public string Address { get; set; } = String.Empty;

        public string City { get; set; } = String.Empty;

        public string Province { get; set; } = String.Empty;

        public string PostalCode { get; set; } = String.Empty;

        public string? ImageUrl { get; set; }

        public double? Score { get; set; }

    }
}
    
       

  
