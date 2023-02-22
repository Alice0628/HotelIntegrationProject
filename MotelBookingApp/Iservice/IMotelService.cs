using MotelBookingApp.Data.ViewModels;
using MotelBookingApp.Models;


namespace MotelBookingApp.Iservice
{
    public interface IMotelService
    {
        Task<List<Motel>> SearchItemAsync(string searchKeyWord);
    
        Task<Motel> Single(int motelId);
        //Task<IEnumerable<Motel>> Find(SearchRequest searchReq);
      
        Task<List<Motel>> GetAll();
        Task<Boolean> DeleteMotel(int motelId);
        Task<Boolean> Add(MotelInputModel entity);

        Task<Boolean> UpdateMotel(MotelInputModel newMotel);
        Task<MotelInputModel> GetEditMotel(int motelId);
        //Task Update(Guid motelId, MotelInputModel entity);

    }
}