
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using MotelBookingApp.Iservice;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Reflection.PortableExecutable;
using MotelBookingApp.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Amazon.DynamoDBv2.Model.Internal.MarshallTransformations;
using Amazon;
using MotelBookingApp.Data;
using Amazon.S3;
using Amazon.S3.Transfer;
using System.IO;
using Amazon.S3.Model;
using MotelBookingApp.Data.ViewModels;
using System.Resources;
using static System.Net.WebRequestMethods;
using Microsoft.AspNetCore.Mvc;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Identity;

namespace MotelBookingApp.Service
{
    public class StaffRoomService : IStaffRoomService
    {
        private string buckName;
        private readonly IAmazonS3 _s3Client;

        private readonly IConfiguration _configue;

        private readonly MotelDbContext _context;
        private readonly IAdminRoomTypeService _adminRoomTypeService;
        private readonly IAdminMotelService _adminMotelService;

        public StaffRoomService(MotelDbContext context, IConfiguration configue, IAdminRoomTypeService adminRoomTypeService, IAdminMotelService adminMotelService)
        {
            buckName = "motelbooking";
            _context = context;
            _configue = configue;

            _s3Client = new AmazonS3Client(_configue.GetValue<string>("AWS:AccessKey"), _configue.GetValue<string>("AWS:AccessKey"), RegionEndpoint.USEast2); ;
            _adminRoomTypeService = adminRoomTypeService;
            _adminMotelService = adminMotelService;
        }

        public async Task<List<Room>> GetAllRooms()
        {
            var rooms = await _context.ScanAsync<Room>(default).GetRemainingAsync();
            return rooms;

        }
        public async Task<List<Room>> SearchRoomAsync(string searchKeyWord)
        {
            var filter = new ScanFilter();

            var res = int.TryParse(searchKeyWord, out int motelId);
            if (res)
            {
                filter.AddCondition("Id", ScanOperator.Equal, motelId);
            }
            else
            {
                filter.AddCondition("RoomNum", ScanOperator.Equal, searchKeyWord);
                filter.AddCondition("RoomType.Name", ScanOperator.Equal, searchKeyWord);
            }
            var config = new ScanOperationConfig
            {
                Filter = filter
            };

            var search = _context.FromScanAsync<Room>(config);
            var results = await search.GetNextSetAsync();

            return results;
        }

        public async Task<Room> SingleRoom(int Id)
        {
            try
            {
                var room = await _context.LoadAsync<Room>(Id);
                if (room == null)
                    return null;
                return room;
            }
            catch (AmazonDynamoDBException ex)
            {
                return null;
            }
        }

        public async Task<Boolean> AddRoom(RoomInputModel newRoom)
        {
            try
            {
                //var requestUpload = new PutObjectRequest
                //{
                //    InputStream = newMotel.RoomImage.OpenReadStream(),
                //    Key = newRoom.RoomImage.FileName,
                //    BucketName = "motelbooking"
                //};

                //await _s3Client.PutObjectAsync(requestUpload);


                RoomType roomType = _adminRoomTypeService.SingleRoomType(newRoom.RoomType).Result;

                //Motel motel = _adminMotelService.SingleMotel(_userManager.GetUserId(User).Result;
                Room room = new Room
                {
                    Id = newRoom.Id,
                    RoomNum = newRoom.RoomNum,

                    Price = newRoom.Price,

                    RoomType = roomType,
                    Motel = new Motel()
 
                };

                await _context.SaveAsync(room);
                return true;
            }
            catch (AmazonDynamoDBException ex)
            {
                return false;
            }

        }

        public async Task<RoomInputModel> GetEditRoom(int roomId)
        {
            Room room = await _context.LoadAsync<Room>(roomId);
            RoomInputModel editRoom = new RoomInputModel();
            List<RoomType> roomTypeList = _adminRoomTypeService.GetAllRoomTypes().Result;
            if (room != null)
            {
                editRoom.Id = room.Id;
                editRoom.RoomNum = room.RoomNum;
                editRoom.Price = room.Price;
                editRoom.Motel = room.Motel;
                editRoom.Type = room.RoomType.Name;
                editRoom.RoomTypeList = roomTypeList;
            
                //editRoom.ImageUrl = room.ImageUrl;
                
                return editRoom;
            }
            else
            {
                return null;
            }
        }

        public async Task<Boolean> UpdateRoom(RoomInputModel newRoom)
        {

            var room = await _context.LoadAsync<Room>(newRoom.Id);
            if (room != null)
            {
                //if (newRoom.RoomImage != null)
                //{
                //    var requestDelete = new DeleteObjectRequest
                //    {
                //        BucketName = buckName,
                //        Key = room.ImageUrl
                //    };

                //    await _s3Client.DeleteObjectAsync(requestDelete);
                //    var requestUpload = new PutObjectRequest
                //    {
                //        InputStream = newRoom.RoomImage.OpenReadStream(),
                //        Key = newRoom.RoomImage.FileName,
                //        BucketName = buckName
                //    };

                //    await _s3Client.PutObjectAsync(requestUpload);

                //    room.ImageUrl = newRoom.RoomImage.FileName;
                //}
                room.RoomNum = newRoom.RoomNum;
                room.Price = newRoom.Price;
                room.RoomType = _adminRoomTypeService.SingleRoomType(newRoom.RoomType).Result;
              
                await _context.SaveAsync(room);
                return true;
            }
            else
            { return false; }
        }


        [HttpDelete]
        public async Task<Boolean> DeleteRoom(int roomId)
        {
            var room = await _context.LoadAsync<Room>(roomId);
            if (room != null)
            {
                //var requestDelete = new DeleteObjectRequest
                //{
                //    BucketName = buckName,
                //    Key = Room.ImageUrl
                //};

                //await _s3Client.DeleteObjectAsync(requestDelete);
                await _context.DeleteAsync(room);
                return true;
            }
            else return false;
        }


        // find the maximum ID value

        public async Task<int> FindMaxIndex()
        {
            var records = await _context.ScanAsync<Room>(default).GetRemainingAsync();

            int maxIndex = 0;
            // Loop through the items to find the maximum ID value
            foreach (var item in records)
            {

                if (item.Id > maxIndex)
                {
                    maxIndex = item.Id;

                }
            }

            return maxIndex;

        }

        //var config = new DynamoDBOperationConfig { IndexName = "AttributeName-index" };
        //var record = await context.LoadAsync<MyRecord>("attributeValue", config);










    }
}

