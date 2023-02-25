
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
    public class AdminMotelService : IAdminMotelService
    {
        private string buckName;
        private readonly IAmazonS3 _s3Client;

        private readonly IConfiguration _configue;

        private readonly MotelDbContext _context;

        public AdminMotelService(MotelDbContext context, IConfiguration configue)
        {
            buckName = "motelbooking";
            _context = context;
            _configue = configue;

            _s3Client = new AmazonS3Client(_configue.GetValue<string>("AWS:AccessKey"), _configue.GetValue<string>("AWS:SecretKey"), RegionEndpoint.USEast2); ;
        }

        public async Task<List<Motel>> GetAllMotels()
        {
            var motels = await _context.ScanAsync<Motel>(default).GetRemainingAsync();
            return motels;

        }
        public async Task<List<Motel>> SearchItemAsync(string searchKeyWord)
        {
            var filter = new ScanFilter();

            var res = int.TryParse(searchKeyWord, out int motelId);
            if (res)
            {
                filter.AddCondition("Id", ScanOperator.Equal, motelId);
            }
            else
            {
                filter.AddCondition("Name", ScanOperator.Equal, searchKeyWord);
            }
            var config = new ScanOperationConfig
            {
                Filter = filter
            };

            var search = _context.FromScanAsync<Motel>(config);
            var results = await search.GetNextSetAsync();

            return results;
        }

        public async Task<Motel> SingleMotel(int Id)
        {
            try
            {
                var motel = await _context.LoadAsync<Motel>(Id);
                if (motel == null)
                    return null;
                return motel;
            }
            catch (AmazonDynamoDBException ex)
            {
                return null;
            }
        }

        public async Task<Boolean> AddMotel(MotelInputModel newMotel)
        {
            try
            {
                var requestUpload = new PutObjectRequest
                {
                    InputStream = newMotel.MotelImage.OpenReadStream(),
                    Key = newMotel.MotelImage.FileName,
                    BucketName = "motelbooking"
                };

                await _s3Client.PutObjectAsync(requestUpload);

                Motel motel = new Motel
                {
                    Id = newMotel.Id,
                    Name = newMotel.Name,

                    Address = newMotel.Address,

                    City = newMotel.City,

                    Province = newMotel.Province,

                    PostalCode = newMotel.PostalCode,

                    ImageUrl = newMotel.MotelImage.FileName

                };

                await _context.SaveAsync(motel);
                return true;
            } catch (AmazonDynamoDBException ex)
            {
                return false;
            }

        }

        public async Task<MotelInputModel> GetEditMotel(int motelId)
        {
            Motel motel = await _context.LoadAsync<Motel>(motelId);
            MotelInputModel editMotel = new MotelInputModel();

            if (motel != null)
            {
                editMotel.Id = motel.Id;
                editMotel.Name = motel.Name;
                editMotel.Address = motel.Address;
                editMotel.Province = motel.Province;
                editMotel.City = motel.City;
                editMotel.PostalCode = motel.PostalCode;
                editMotel.ImageUrl = motel.ImageUrl;
                editMotel.ImageUrl = $"https://motelbooking.s3.us-east-2.amazonaws.com/{motel.ImageUrl}";
                return editMotel;
            }
            else
            {
                return null;
            }
        }

        public async Task<Boolean> UpdateMotel(MotelInputModel newMotel)
        {

            var motel = await _context.LoadAsync<Motel>(newMotel.Id);
            if (motel != null)
            {
                if (newMotel.MotelImage != null)
                {
                    var requestDelete = new DeleteObjectRequest
                    {
                        BucketName = buckName,
                        Key = motel.ImageUrl
                    };

                    await _s3Client.DeleteObjectAsync(requestDelete);
                    var requestUpload = new PutObjectRequest
                    {
                        InputStream = newMotel.MotelImage.OpenReadStream(),
                        Key = newMotel.MotelImage.FileName,
                        BucketName = buckName
                    };

                    await _s3Client.PutObjectAsync(requestUpload);

                    motel.ImageUrl = newMotel.MotelImage.FileName;
                }
                motel.Name = newMotel.Name;
                motel.Address = newMotel.Address;
                motel.City = newMotel.City;
                motel.Province = newMotel.Province;
                motel.PostalCode = newMotel.PostalCode;
                await _context.SaveAsync(motel);
                return true;
            }
            else
            { return false; }
        }


        [HttpDelete]
        public async Task<Boolean> DeleteMotel(int motelId)
        {
            var motel = await _context.LoadAsync<Motel>(motelId);
            if (motel != null)
            {
                var requestDelete = new DeleteObjectRequest
                {
                    BucketName = buckName,
                    Key = motel.ImageUrl
                };

                await _s3Client.DeleteObjectAsync(requestDelete);
                await _context.DeleteAsync(motel);
                return true;
            }
            else return false;
        }


        // find the maximum ID value

        public async Task<int> FindMaxIndex()
        {
            var records = await _context.ScanAsync<Motel>(default).GetRemainingAsync();

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

