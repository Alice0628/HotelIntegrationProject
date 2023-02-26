
namespace MotelBookingApp.Models
{

    public class RoomType
    {
      
        public int Id { get; set; } = default;
       
        public string Name { get; set; } = String.Empty;
      
        public int Sleep { get; set; } = default;
      
        public string? ImageUrl { get; set; }
      
        public string? Description { get; set; }
    }
}
