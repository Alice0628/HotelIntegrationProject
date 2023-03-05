namespace MotelBookingApp.Models
{
    public class FavoriteMotelList
    {
        public int Id { get; set; }

        public int MotelId { get; set; }

        public Motel Motel { get; set; }

        public AppUser Owner { get; set; }
    }
}
