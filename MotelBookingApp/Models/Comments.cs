using Amazon.DynamoDBv2.DataModel;

namespace MotelBookingApp.Models
{
    public class Comments
    {
        public int Id { get; set; } = default;

        public AppUser? User { get; set; } = new AppUser();
       
        public string Content { get; set; } = String.Empty;
       
        public DateTime CreateDate { get; set; } = DateTime.Now;
      
        public string? Score { get; set; } = String.Empty;

        public Motel Motel { get; set; }
    }
}
