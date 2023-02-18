namespace MotelBkApp.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public AppUser AppUser { get; set; }
        public string ConfirmCode { get; set; }

        public DateTime PayTime { get; set; }

        public decimal TotalAmount { get; set; }
    }
}
