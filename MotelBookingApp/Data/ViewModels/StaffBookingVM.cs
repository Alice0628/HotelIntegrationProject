using MotelBookingApp.Models;

namespace MotelBookingApp.Data.ViewModels
{
    public class StaffBookingVM
    {
        public List<RoomInputModel>? AvailableRooms { get; set; }

        public List<RoomType>? RoomTypeList { get; set; }  

        public string? SearchType { get; set; } 
        public string? City { get; set; }

        public DateTime? CheckinDate { get; set; }

        public DateTime? CheckoutDate { get; set; }
    }
}
