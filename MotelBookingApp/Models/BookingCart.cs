namespace MotelBookingApp.Models
{
    public class BookingCart
    {
        public int Id { get; set; } = default;
        public AppUser AppUser { get; set; } = new AppUser();
        public DateTime CheckinDate { get; set; } = DateTime.Now;
        public DateTime CheckoutDate { get; set; } = DateTime.Now.AddDays(1);
        public string OccupantName { get; set; } = String.Empty;
        public string Notes { get; set; } = String.Empty;
        public RoomType Type { get; set; } = new RoomType();
    }
}
