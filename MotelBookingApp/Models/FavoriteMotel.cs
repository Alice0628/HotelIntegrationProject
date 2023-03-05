namespace MotelBookingApp.Models
{
    public class FavoriteMotel
    {
        public int Id { get; set; } 
        public int MotelId { get; set; }
        public AppUser? AppUser { get; set; }
        public Motel? Motel { get; set; }
    }

}
