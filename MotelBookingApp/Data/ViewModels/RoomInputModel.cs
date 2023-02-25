using Amazon.DynamoDBv2.DataModel;
using MotelBookingApp.Models;

namespace MotelBookingApp.Data.ViewModels
{
    public class RoomInputModel
    {
        public int Id { get; set; } = default;
        public string RoomNum { get; set; } = String.Empty;
    
        public decimal Price { get; set; } = default;
     
        public int RoomType { get; set; } = default;

        public string? Type { get; set; } = String.Empty;

        public Motel? Motel { get; set; } = default;

        public List<RoomType> RoomTypeList { get; set; } = new List<RoomType>();
    }
}
