
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

namespace MotelBookingApp.Service
{
    public class AdminRoomTypeService : IAdminRoomTypeService
    {
        private string buckName;
        private readonly IAmazonS3 _s3Client;

        private readonly IConfiguration _configue;

        private readonly MotelDbContext _context;

        public AdminRoomTypeService(MotelDbContext context, IConfiguration configue)
        {
            buckName = "motelbooking";
            _context = context;
            _configue = configue;

            _s3Client = new AmazonS3Client(_configue.GetValue<string>("AWS:AccessKey"), _configue.GetValue<string>("AWS:SecretKey"), RegionEndpoint.USEast2); ;
        }

        public async Task<List<RoomType>> GetAllRoomTypes()
        {
            var roomTypes = await _context.ScanAsync<RoomType>(default).GetRemainingAsync();
            return roomTypes;

        }
        public async Task<List<RoomType>> SearchRoomTypeAsync(string searchKeyWord)
        {
            var filter = new ScanFilter();

            var res = int.TryParse(searchKeyWord, out int roomTypeId);
            if (res)
            {
                filter.AddCondition("Id", ScanOperator.Equal, roomTypeId);
            }
            else
            {
                filter.AddCondition("Name", ScanOperator.Equal, searchKeyWord);
            }
            var config = new ScanOperationConfig
            {
                Filter = filter
            };

            var search = _context.FromScanAsync<RoomType>(config);
            var results = await search.GetNextSetAsync();

            return results;
        }

        public async Task<RoomType> SingleRoomType(int Id)
        {
            try
            {
                var roomType = await _context.LoadAsync<RoomType>(Id);
                if (roomType == null)
                    return null;
                return roomType;
            }
            catch (AmazonDynamoDBException ex)
            {
                return null;
            }
        }

        public async Task<Boolean> AddRoomType(RoomTypeInputModel newRoomType)
        {
            try
            {
                var requestUpload = new PutObjectRequest
                {
                    InputStream = newRoomType.TypeImage.OpenReadStream(),
                    Key = newRoomType.TypeImage.FileName,
                    BucketName = "motelbooking"
                };

                await _s3Client.PutObjectAsync(requestUpload);

                RoomType roomType = new RoomType
                {
                    Id = newRoomType.Id,
                    Name = newRoomType.Name,

                    Description = newRoomType.Description,

                    Sleep = newRoomType.Sleep,
 
                    ImageUrl = newRoomType.TypeImage.FileName

                };

                await _context.SaveAsync(roomType);
                return true;
            } catch (AmazonDynamoDBException ex)
            {
                return false;
            }

        }

        public async Task<RoomTypeInputModel> GetEditRoomType(int roomTypeId)
        {
            RoomType roomType = await _context.LoadAsync<RoomType>(roomTypeId);
            RoomTypeInputModel editRoomType = new RoomTypeInputModel();

            if (editRoomType != null)
            {
                editRoomType.Id = roomType.Id;
                editRoomType.Name = roomType.Name;
                editRoomType.Sleep = roomType.Sleep;
                editRoomType.Description = roomType.Description;
                editRoomType.ImageUrl = $"https://motelbooking.s3.us-east-2.amazonaws.com/{roomType.ImageUrl}"; 
                return editRoomType;
            }
            else
            {
                return null;
            }
        }

        public async Task<Boolean> UpdateRoomType(RoomTypeInputModel newRoomType)
        {

            var roomType = await _context.LoadAsync<RoomType>(newRoomType.Id);
            if (roomType != null)
            {
                if (newRoomType.TypeImage != null)
                {
                    var requestDelete = new DeleteObjectRequest
                    {
                        BucketName = buckName,
                        Key = roomType.ImageUrl
                    };

                    await _s3Client.DeleteObjectAsync(requestDelete);
                    var requestUpload = new PutObjectRequest
                    {
                        InputStream = newRoomType.TypeImage.OpenReadStream(),
                        Key = newRoomType.TypeImage.FileName,
                        BucketName = buckName
                    };

                    await _s3Client.PutObjectAsync(requestUpload);

                    roomType.ImageUrl = newRoomType.TypeImage.FileName;
                }
                roomType.Name = newRoomType.Name;
                roomType.Sleep = newRoomType.Sleep;
                roomType.Description = newRoomType.Description;
                 
                await _context.SaveAsync(roomType);
                return true;
            }
            else
            { return false; }
        }


        [HttpDelete]
        public async Task<Boolean> DeleteRoomType(int roomTypeId)
        {
            var roomType = await _context.LoadAsync<RoomType>(roomTypeId);
            if (roomType != null)
            {
                var requestDelete = new DeleteObjectRequest
                {
                    BucketName = buckName,
                    Key = roomType.ImageUrl
                };

                await _s3Client.DeleteObjectAsync(requestDelete);
                await _context.DeleteAsync(roomType);
                
                return true;
            }
            else return false;
        }


        // find the maximum ID value

        public async Task<int> FindMaxIndex()
        {
            var records = await _context.ScanAsync<RoomType>(default).GetRemainingAsync();

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

