 
using MotelBookingApp.Models;

namespace MotelBookingApp.Data.ViewModels
{
    public class MotelDetailModel
    {
        public Motel Motel { get; set; }
        public List<Comment> Comments { get; set; }
    }
}
