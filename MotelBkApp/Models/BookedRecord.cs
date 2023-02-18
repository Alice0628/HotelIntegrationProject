namespace MotelBkApp.Models
{
    public class BookedRecord
    {
        public int Id { get; set; }
        public DateTime CheckinDate { get; set; }

        public DateTime CheckoutDate { get; set; }

        public string OccupantName { get; set; }

        public RoomType Type { get; set; }

        public decimal price { get; set; }

        public decimal SubTotal { get; set; }

        public Booking Booking { get; set; }
    }
}
