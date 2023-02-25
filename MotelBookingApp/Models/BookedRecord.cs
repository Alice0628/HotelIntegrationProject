using Amazon.DynamoDBv2.DataModel;

namespace MotelBookingApp.Models
{
    [DynamoDBTable("BookedRecords")]
    public class BookedRecord
    {
        [DynamoDBHashKey]
        public int Id { get; set; } = default;
        [DynamoDBProperty]
        public DateTime CheckinDate { get; set; } = DateTime.Now;
        [DynamoDBProperty]
        public DateTime CheckoutDate { get; set; } = DateTime.Now.AddDays(1);
        [DynamoDBProperty]
        public string OccupantName { get; set; } = String.Empty;
        [DynamoDBProperty]
        public Room Room { get; set; } = new Room();
        [DynamoDBProperty]
        public decimal Price { get; set; } = default;
        [DynamoDBProperty]
        public Booking Booking { get; set; } = new Booking();
    }
}
