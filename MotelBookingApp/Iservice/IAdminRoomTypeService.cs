using MotelBookingApp.Data.ViewModels;
using MotelBookingApp.Models;


namespace MotelBookingApp.Iservice
{
    public interface IAdminRoomTypeService
    {
        Task<List<RoomType>> SearchRoomTypeAsync(string searchKeyWord);
    
        Task<RoomType> SingleRoomType(int motelId);
        //Task<IEnumerable<Motel>> Find(SearchRequest searchReq);
      
        Task<List<RoomType>> GetAllRoomTypes();
        Task<Boolean> DeleteRoomType(int motelId);
        Task<Boolean> AddRoomType(RoomTypeInputModel entity);

        Task<int> FindMaxIndex();

        Task<Boolean> UpdateRoomType(RoomTypeInputModel newRoomType);
        Task<RoomTypeInputModel> GetEditRoomType(int roomTypeId);
        //Task Update(Guid motelId, MotelInputModel entity);

    }
}