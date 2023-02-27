
using MotelBookingApp.Models;

namespace MotelBookingApp.Data.ViewModels
{
    public class RoomInputModel
    {
        public int Id { get; set; } = default;
        public string RoomNum { get; set; } = String.Empty;
    
        public decimal Price { get; set; } = default;
     
        public int RoomType { get; set; } = default;

        public string RoomTypeName { get; set; }

        public string? RoomTypeImage { get; set; } = String.Empty;
 
        public string? MotelName { get; set; } = string.Empty;

        public List<RoomType> RoomTypeList { get; set; } = new List<RoomType>();
    }
}
