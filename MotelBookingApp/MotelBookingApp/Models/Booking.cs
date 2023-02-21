namespace MotelBookingApp.Models
{
    public class Booking
    {
        public int Id { get; set; } = default;
        public AppUser AppUser { get; set; } = new AppUser();
        public string ConfirmCode { get; set; } = String.Empty;
        public DateTime PayTime { get; set; } = DateTime.Now;
        public decimal TotalAmount { get; set; } = default;
    }
}
