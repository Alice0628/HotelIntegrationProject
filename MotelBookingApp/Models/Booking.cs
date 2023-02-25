using Amazon.DynamoDBv2.DataModel;

namespace MotelBookingApp.Models
{
    [DynamoDBTable("Bookings")]
    public class Booking
    {
        [DynamoDBHashKey]
        public int Id { get; set; } = default;
        [DynamoDBProperty]
        public AppUser AppUser { get; set; } = new AppUser();
        [DynamoDBProperty]
        public string ConfirmCode { get; set; } = String.Empty;
        [DynamoDBProperty]
        public DateTime PayTime { get; set; } = DateTime.Now;
        [DynamoDBProperty]
        public decimal TotalAmount { get; set; } = default;
    }
}
