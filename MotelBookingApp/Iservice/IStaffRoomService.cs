using MotelBookingApp.Data.ViewModels;
using MotelBookingApp.Models;

namespace MotelBookingApp.Iservice
{
    public interface IStaffRoomService
    {

        Task<List<Room>> SearchRoomAsync(string searchKeyWord);

        Task<Room> SingleRoom(int roomlId);
        //Task<IEnumerable<Motel>> Find(SearchRequest searchReq);

        Task<List<Room>> GetAllRooms();
        Task<Boolean> DeleteRoom(int roomId);
        Task<Boolean> AddRoom(RoomInputModel entity);

        Task<int> FindMaxIndex();

        Task<Boolean> UpdateRoom(RoomInputModel newRoom);
        Task<RoomInputModel> GetEditRoom(int roomId);
        //Task Update(Guid motelId, MotelInputModel entity);
    }
}
