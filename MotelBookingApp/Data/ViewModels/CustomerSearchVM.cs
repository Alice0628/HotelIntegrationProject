using MotelBookingApp.Models;

namespace MotelBookingApp.Data.ViewModels
{
    public class CustomerSearchVM
    {
        public List<MotelInputModel>? AvailableMotels {get;set; }
        public List<RoomType>? RoomTypeList { get; set; }
        public string? RoomTypeName { get; set; }

        public string? SearchType { get; set; }
        public string? City { get; set; }

        public DateTime? CheckinDate { get; set; }

        public DateTime? CheckoutDate { get; set; }
    }
}

