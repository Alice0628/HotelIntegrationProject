namespace MotelBkApp.Models
{
    public class BookingCart
    {
        public int Id { get; set; }
        public AppUser AppUser { get; set; }
        public DateTime CheckinDate { get; set; }

        public DateTime CheckoutDate { get; set; }

        public string OccupantName { get; set; }

        public string Notes { get; set; }

        public RoomType Type { get; set; }
    }
}
