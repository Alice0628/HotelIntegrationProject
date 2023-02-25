using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;



namespace MotelBookingApp.Data
{
    public class MotelDbContext : DynamoDBContext
    {
        public MotelDbContext(IAmazonDynamoDB client, DynamoDBContextConfig config)
       : base(client, config)
        {
        }


    }
}