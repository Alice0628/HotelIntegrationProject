using Amazon.DynamoDBv2.DataModel;

namespace MotelBookingApp.Models
{
    [DynamoDBTable("BookingCarts")]
    public class BookingCart
    {
        [DynamoDBHashKey]
        public int Id { get; set; } = default;
        [DynamoDBProperty]
        public AppUser AppUser { get; set; } = new AppUser();
        [DynamoDBProperty]
        public DateTime CheckinDate { get; set; } = DateTime.Now;
        [DynamoDBProperty]
        public DateTime CheckoutDate { get; set; } = DateTime.Now.AddDays(1);
        [DynamoDBProperty]
        public string OccupantName { get; set; } = String.Empty;
        [DynamoDBProperty]
        public string Notes { get; set; } = String.Empty;
        [DynamoDBProperty]
        public Room Room { get; set; } = new Room();
    }
}
