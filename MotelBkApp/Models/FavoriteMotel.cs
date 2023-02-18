namespace MotelBkApp.Models
{
    public class FavoriteMotel
    {
        public int Id { get; set; } = default;
        public AppUser AppUser { get; set; } = new AppUser();
        public Motel Motel { get; set; } = new Motel();
    }
}
