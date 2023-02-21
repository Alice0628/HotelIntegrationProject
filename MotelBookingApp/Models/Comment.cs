namespace MotelBookingApp.Models
{
    public class Comment
    {
        public int Id { get; set; } = default;
        public Motel Motel { get; set; } = new Motel();
        public AppUser AppUser { get; set; } = new AppUser();
        public string Content { get; set; } = String.Empty; 
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public double Score { get; set; } = default;
    }
}
