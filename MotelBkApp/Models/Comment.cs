namespace MotelBkApp.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public Motel Motel { get; set; }
        public AppUser AppUser { get; set; }

        public string Content { get; set; }

        public DateTime CreateTime { get; set; }

        public double Score { get; set; }
    }
}
