namespace MotelBkApp.Models
{
    public class RoomType
    {   
        public int Id { get; set; }
        public string Name { get; set; }
        public string Price { get; set; }
        public string Description { get; set; }

        public string ImageUrl { get; set; }

        public int Sleep { get; set; }

        public RoomType Type { get; set; }
    }
}
