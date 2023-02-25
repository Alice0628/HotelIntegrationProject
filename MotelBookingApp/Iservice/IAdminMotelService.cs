using MotelBookingApp.Data.ViewModels;
using MotelBookingApp.Models;


namespace MotelBookingApp.Iservice
{
    public interface IAdminMotelService
    {
        Task<List<Motel>> SearchItemAsync(string searchKeyWord);
    
        Task<Motel> SingleMotel(int motelId);
        //Task<IEnumerable<Motel>> Find(SearchRequest searchReq);
      
        Task<List<Motel>> GetAllMotels();
        Task<Boolean> DeleteMotel(int motelId);
        Task<Boolean> AddMotel(MotelInputModel entity);

        Task<int> FindMaxIndex();

        Task<Boolean> UpdateMotel(MotelInputModel newMotel);
        Task<MotelInputModel> GetEditMotel(int motelId);
        //Task Update(Guid motelId, MotelInputModel entity);

    }
}