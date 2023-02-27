 
using MotelBookingApp.Models;

namespace MotelBookingApp.Data.ViewModels
{
    public class MotelDetailModel
    {
        public MotelInputModel Motel { get; set; }
        public List<Comment>? Comments { get; set; }
    }
}
