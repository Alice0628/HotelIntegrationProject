namespace MotelBkApp.Models
{
    public class Motel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }

        public string City { get; set; }

        public string Province { get; set; }

        public string PostalCode { get; set; }

        public string ImageUrl { get; set; }

        public double? Score { get; set; }
    }
}
