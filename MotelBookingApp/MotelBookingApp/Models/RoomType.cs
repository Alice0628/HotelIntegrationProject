namespace MotelBookingApp.Models
{
    public class RoomType
    {
        public int Id { get; set; } = default;
        public string Name { get; set; } = String.Empty;
        public string Price { get; set; } = String.Empty;
        public string Description { get; set; } = String.Empty;
        public string ImageUrl { get; set; } = String.Empty;
        public int Sleep { get; set; } = default;
        public int Amount { get; set; } = default;
        public Motel Motel { get; set; } = new Motel();
    }
}
