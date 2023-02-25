using Amazon.DynamoDBv2.DataModel;
using System.ComponentModel.DataAnnotations;

namespace MotelBookingApp.Models
{
    [DynamoDBTable("Motels")]
    public class Motel
    {
        [DynamoDBHashKey]
        public int Id { get; set; } = default;
        [DynamoDBProperty]
        public string Name { get; set; } = String.Empty;
        [DynamoDBProperty]
        public string Address { get; set; } = String.Empty;
        [DynamoDBProperty]
        public string City { get; set; } = String.Empty;
        [DynamoDBProperty]
        public string Province { get; set; } = String.Empty;
        [DynamoDBProperty]
        public string PostalCode { get; set; } = String.Empty;
        [DynamoDBProperty]
        public string ImageUrl { get; set; } = String.Empty;
        [DynamoDBProperty]
        public double? Score { get; set; } = default;
        [DynamoDBProperty]
        public List<Comment>? Comments { get; set; } = new List<Comment>();
    }

        public class Comment
        {
        public int Id { get; set; } = default;
        public AppUser User { get; set; } = new AppUser();
        public string Content { get; set; } = String.Empty;
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string? Score {get; set; } = String.Empty;
        }
}


  