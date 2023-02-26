

namespace MotelBookingApp.Models
{
   
    public class Room
    {
      
        public int Id { get; set; } = default;
      
        public Motel Motel { get; set; } = new Motel();
 
        public string RoomNum { get; set; } = String.Empty;
       
        public decimal Price { get; set; } = default;
    
        public RoomType RoomType { get; set; } = new RoomType();

        public Boolean IfAvailable { get; set; } = true;
  
    }

   
}
