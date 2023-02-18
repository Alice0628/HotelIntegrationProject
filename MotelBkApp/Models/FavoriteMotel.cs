namespace MotelBkApp.Models
{
    public class FavoriteMotel
    {
        public int Id { get; set; }
        public AppUser AppUser { get; set; }
        public Motel Motel { get; set; }
    }
}
