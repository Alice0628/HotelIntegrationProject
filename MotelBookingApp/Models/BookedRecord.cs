

namespace MotelBookingApp.Models
{

    public class BookedRecord
    {
    
        public int Id { get; set; } = default;
  
        public DateTime CheckinDate { get; set; } = DateTime.Now;
    
        public DateTime CheckoutDate { get; set; } = DateTime.Now.AddDays(1);
        
        public string OccupantName { get; set; } = String.Empty;
      
        public Room Room { get; set; } = new Room();
        
        public decimal Price { get; set; } = default;
    
        public Booking Booking { get; set; } = new Booking();
    }
}
