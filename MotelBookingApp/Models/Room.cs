using Amazon.DynamoDBv2.DataModel;

namespace MotelBookingApp.Models
{
    [DynamoDBTable("Rooms")]
    public class Room
    {
        [DynamoDBHashKey]
        public int Id { get; set; } = default;
        [DynamoDBProperty]
        public Motel Motel { get; set; } = new Motel();
        [DynamoDBProperty]
        public string RoomNum { get; set; } = String.Empty;
        [DynamoDBProperty]
        public decimal Price { get; set; } = default;
        [DynamoDBProperty]
        public RoomType RoomType { get; set; } = new RoomType();

        public Boolean IfAvailable { get; set; } = true;
  
    }

   
}
