using MotelBookingApp.Models;

namespace MotelBookingApp.Data.ViewModels
{
    public class BookedRecordDisplayVM
    {
        public List<BookedRecord>? BookedRooms { get; set; }

        public List<RoomType>? RoomTypeList { get; set; }

        public string RoomTypeName { get; set; }
     
        public string? SearchType { get; set; }
        public string? UserName { get; set; }

        public DateTime? CheckinDate { get; set; }

        public DateTime? CheckoutDate { get; set; }
    }
}
