//using Amazon.DynamoDBv2.DataModel;
//using Amazon.DynamoDBv2;
using MotelBookingApp.Iservice;
//using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Reflection.PortableExecutable;
using MotelBookingApp.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
//using Amazon.DynamoDBv2.Model.Internal.MarshallTransformations;
//using Amazon;
using MotelBookingApp.Data;
//using Amazon.S3;
//using Amazon.S3.Transfer;
using System.IO;
//using Amazon.S3.Model;
using MotelBookingApp.Data.ViewModels;
using System.Resources;
using static System.Net.WebRequestMethods;
using Microsoft.AspNetCore.Mvc;
using System.Drawing.Drawing2D;
using System.Security.Policy;
//using Amazon.DynamoDBv2.Model;


namespace MotelBookingApp.Service
{
    public class MotelService : IMotelService
    {
        //private string buckName;
        //private readonly IAmazonS3 _s3Client;


        private readonly MotelDbContext _context;

        public MotelService(MotelDbContext context)
        {
            //buckName = "motelbooking";
            _context = context;
            //_s3Client = new AmazonS3Client("AKIAULQQJCLCEDFQ3CX7", "Si4UB395yY6MH9+hsw5M7cfS3hlIVPxOmtKHdpK+", RegionEndpoint.USEast2); ;
        }

        public async Task<List<Motel>> SearchItemAsync(string searchKeyWord)
        {
            if (searchKeyWord == null)
            {
                return null;
            }
            else
            {
                try
                {
                    searchKeyWord = searchKeyWord.ToLower();
                    List<Motel> motels = await _context.Motels.Where(a => a.Name.ToLower().Contains(searchKeyWord) || a.City.ToLower().Contains(searchKeyWord) || a.PostalCode.ToLower().Contains(searchKeyWord)).ToListAsync();
                    if (motels != null)
                        return motels;
                    else
                        return null;
                }
                catch (SystemException ex)
                {
                    return null;
                }

            }
        }

        public async Task<Motel> Single(int Id)
        {
            try
            {
                var motel = await _context.Motels.FirstOrDefaultAsync(a => a.Id == Id);
                if (motel == null)
                    return null;
                return motel;
            }      
            catch (SystemException ex)
            {
                return null;
            }
        }


        [HttpDelete("{motelId}")]
        public async Task<Boolean> DeleteMotel(int motelId)
        {
            try
            {
                var motel = await _context.Motels.FindAsync(motelId);
                if (motel != null)
                {
                    var fileNameOld = Path.GetFileName(motel.ImageUrl);
                    var filePathOld = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileNameOld);
                    if (System.IO.File.Exists(filePathOld))
                    {
                        System.IO.File.Delete(filePathOld);
                    }
                    _context.Motels.Remove(motel);
                    _context.SaveChanges();

                    return true;
                }
                return false;
            }
            catch (SystemException ex) {
                return false;
            }
        }
     
      
        public async Task<Boolean> UpdateMotel(MotelInputModel newMotel)
        {

            var motel= await _context.Motels.FirstOrDefaultAsync(m => m.Id == newMotel.Id);
            if (motel != null)
            {
              if (newMotel.MotelImage != null)
                {
                        var fileNameOld = Path.GetFileName(motel.ImageUrl);
                        var fileNameNew = Path.GetFileName(newMotel.MotelImage.FileName);
                        var filePathOld = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileNameOld);
                        if (System.IO.File.Exists(filePathOld))
                        {
                            System.IO.File.Delete(filePathOld);
                        }
                    var filePathNew = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileNameNew);
                    using (var fileStream = new FileStream(filePathNew, FileMode.Create))
                        {
                            await newMotel.MotelImage.CopyToAsync(fileStream);
                        }
                        motel.ImageUrl = newMotel.MotelImage.FileName;
                    }
              motel.Name = newMotel.Name;
              motel.Address = newMotel.Address;
              motel.City = newMotel.City;
              motel.Province = newMotel.Province;
              motel.PostalCode = newMotel.PostalCode;
              _context.Motels.Update(motel);
              _context.SaveChanges();
              return true;
            }
            else
            { return false; }
        }

        public async Task<List<Motel>> GetAll()
        {
            try
            {
                var motels = await _context.Motels.ToListAsync();
                return motels;
            }
            catch (SystemException ex) 
            {
                return null;
            }

        }


        [HttpPost]
        public async Task<Boolean> Add(MotelInputModel newMotel)
        {
            var ImageUrl = "";
            if (newMotel.MotelImage != null)
            {
                var fileNameNew = Path.GetFileName(newMotel.MotelImage.FileName);
                var filePathNew = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileNameNew);
                using (var fileStream = new FileStream(filePathNew, FileMode.Create))
                {
                    await newMotel.MotelImage.CopyToAsync(fileStream);
                }
                ImageUrl = newMotel.MotelImage.FileName;

            }
 
            Motel motel = new Motel
                {
                    Id = newMotel.Id,
                    Name = newMotel.Name,

                    Address = newMotel.Address,

                    City = newMotel.City,

                    Province = newMotel.Province,

                    PostalCode = newMotel.PostalCode,

                    ImageUrl = ImageUrl 

            };
                try
                {
                _context.Motels.Add(motel);
                await _context.SaveChangesAsync();
                return true;
                }
            catch (SystemException ex)
            {
                return false;
            }
                
            
        }
 
        public async Task<MotelInputModel> GetEditMotel(int motelId)
        {
            Motel motel = await _context.Motels.FirstOrDefaultAsync(m => m.Id == motelId);
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
                return editMotel;
            }
            else
            {
                return null;
            }
        }

    }
}
