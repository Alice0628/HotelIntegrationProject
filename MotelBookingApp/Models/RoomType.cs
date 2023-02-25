using Amazon.DynamoDBv2.DataModel;

namespace MotelBookingApp.Models
{
    [DynamoDBTable("RoomTypes")]
    public class RoomType
    {
        [DynamoDBHashKey]
        public int Id { get; set; } = default;
        [DynamoDBProperty]
        public string Name { get; set; } = String.Empty;
        [DynamoDBProperty]
        public int Sleep { get; set; } = default;
        [DynamoDBProperty]
        public string ImageUrl { get; set; } = String.Empty;
        [DynamoDBProperty]
        public string? Description { get; set; } = String.Empty;
    }
}
