namespace MotelBkApp.Models
{
    public class BookedRecord
    {
        public int Id { get; set; } = default;
        public DateTime CheckinDate { get; set; } = DateTime.Now;
        public DateTime CheckoutDate { get; set; } = DateTime.Now.AddDays(1);
        public string OccupantName { get; set; } = String.Empty;
        public RoomType Type { get; set; } = new RoomType();
        public decimal price { get; set; } = default;
        public decimal SubTotal { get; set; } = default;
        public Booking Booking { get; set; } = new Booking();
    }
}
