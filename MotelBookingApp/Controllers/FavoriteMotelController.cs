//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;
//using MotelBookingApp.Data;
//using MotelBookingApp.Data.ViewModels;
//using MotelBookingApp.Iservice;
//using MotelBookingApp.Models;

//namespace MotelBookingApp.Controllers
//{
//    public class FavoriteMotelController : Controller
//    {
//        private readonly MotelDbContext _context;
//        private IMotelService _motelService;


//        public FavoriteMotelController(MotelDbContext context, IMotelService repository)
//        {
//            _context = context;
//            _motelService = repository;
//        }

//        // GET: FavoriteMotel
//        public async Task<IActionResult> Index()
//        {
//            //return _context.FavoriteMotels != null ?
//            //            View(await _context.FavoriteMotels.Include(m =>m.Motel).ToListAsync()) :
//            //            Problem("Entity set 'MotelDbContext.FavoriteMotels'  is null.");
//            var motelAppContext = _context.FavoriteMotels.Include(r => r.Motel);
//            return View(await motelAppContext.ToListAsync());

//        }

//        public async Task<IActionResult> MotelList()
//        {

//            var motels = await _motelService.GetAll();
//            foreach (var m in motels)
//            {
//                m.ImageUrl = Url.Content("~/images/" + m.ImageUrl);
//            }
//            return View(motels);

//        }

//        // GET: FavoriteMotel/Details/5
//        public async Task<IActionResult> Details(int? id)
//        {
//            if (id == null || _context.FavoriteMotels == null)
//            {
//                return NotFound();
//            }

//            var favoriteMotel = await _context.FavoriteMotels
//                .FirstOrDefaultAsync(m => m.Id == id);
//            if (favoriteMotel == null)
//            {
//                return NotFound();
//            }

//            return View(favoriteMotel);
//        }





//        [BindProperty]
//        public int MotelId { get; set; }

//        [BindProperty]
//        public Motel Motel { get; set; }

//        [BindProperty]
//        public FavoriteMotel FavoriteMotel { get; set; }



//        // GET: FavoriteMotel/Create
//        public async Task<IActionResult> Create(int Id, FavoriteMotel favoriteMotel)
//        {

//          Motel  Motel = await _context.Motels.FindAsync(Id);

//            favoriteMotel.ModelId = Motel.Id;


//            if (Motel != null)
//            {



//            }

//            return View(favoriteMotel);

//        }



//    // POST: FavoriteMotel/Create
//    // To protect from overposting attacks, enable the specific properties you want to bind to.
//    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
//    [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create([Bind("Id,UserId,ModelId")] FavoriteMotel favoriteMotel)
//        {


//            //var userName = User.Identity.Name;
//            //var currentUser = _context.Users.Where(u => u.Email == userName).FirstOrDefault();
//            //favoriteMotel.AppUser = currentUser;




//            if (ModelState.IsValid)
//            {
//                _context.Add(favoriteMotel);
//                await _context.SaveChangesAsync();
//                return RedirectToAction(nameof(Index));
//            }
//            return View(favoriteMotel);
//        }

//        // GET: FavoriteMotel/Edit/5
//        public async Task<IActionResult> Edit(int? id)
//        {
//            if (id == null || _context.FavoriteMotels == null)
//            {
//                return NotFound();
//            }

//            var favoriteMotel = await _context.FavoriteMotels.FindAsync(id);
//            if (favoriteMotel == null)
//            {
//                return NotFound();
//            }
//            return View(favoriteMotel);
//        }

//        // POST: FavoriteMotel/Edit/5
//        // To protect from overposting attacks, enable the specific properties you want to bind to.
//        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,ModelId")] FavoriteMotel favoriteMotel)
//        {
//            if (id != favoriteMotel.Id)
//            {
//                return NotFound();
//            }

//            if (ModelState.IsValid)
//            {
//                try
//                {
//                    _context.Update(favoriteMotel);
//                    await _context.SaveChangesAsync();
//                }
//                catch (DbUpdateConcurrencyException)
//                {
//                    if (!FavoriteMotelExists(favoriteMotel.Id))
//                    {
//                        return NotFound();
//                    }
//                    else
//                    {
//                        throw;
//                    }
//                }
//                return RedirectToAction(nameof(Index));
//            }
//            return View(favoriteMotel);
//        }

//        // GET: FavoriteMotel/Delete/5
//        public async Task<IActionResult> Delete(int? id)
//        {
//            if (id == null || _context.FavoriteMotels == null)
//            {
//                return NotFound();
//            }

//            var favoriteMotel = await _context.FavoriteMotels
//                .FirstOrDefaultAsync(m => m.Id == id);
//            if (favoriteMotel == null)
//            {
//                return NotFound();
//            }

//            return View(favoriteMotel);
//        }

//        // POST: FavoriteMotel/Delete/5
//        [HttpPost, ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteConfirmed(int id)
//        {
//            if (_context.FavoriteMotels == null)
//            {
//                return Problem("Entity set 'MotelDbContext.FavoriteMotels'  is null.");
//            }
//            var favoriteMotel = await _context.FavoriteMotels.FindAsync(id);
//            if (favoriteMotel != null)
//            {
//                _context.FavoriteMotels.Remove(favoriteMotel);
//            }

//            await _context.SaveChangesAsync();
//            return RedirectToAction(nameof(Index));
//        }

//        private bool FavoriteMotelExists(int id)
//        {
//            return (_context.FavoriteMotels?.Any(e => e.Id == id)).GetValueOrDefault();
//        }
//    }
//}




using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MotelBookingApp.Data;
using MotelBookingApp.Models;

namespace MotelBookingApp
{
    public class FavoriteMotelController : Controller
    {
        private readonly MotelDbContext _context;


        public FavoriteMotelController(MotelDbContext context)
        {
            _context = context;
        }

        // GET: FavoriteMotel
        public async Task<IActionResult> Index()
        {
            //return _context.FavoriteMotels != null ?
            //            View(await _context.FavoriteMotels.Include(m =>m.Motel).ToListAsync()) :
            //            Problem("Entity set 'MotelDbContext.FavoriteMotel'  is null.");
            var userName = User.Identity.Name;
            var motelAppContext = _context.FavoriteMotels.Include(r => r.Motel).Where(a => a.AppUser.UserName ==userName);
            return View(await motelAppContext.ToListAsync());
        }

        public async Task<IActionResult> FavoriteMotelList()
        {
            //return _context.FavoriteMotels != null ?
            //            View(await _context.FavoriteMotels.Include(m =>m.Motel).ToListAsync()) :
            //            Problem("Entity set 'MotelDbContext.FavoriteMotel'  is null.");

            var motelAppContext = _context.FavoriteMotels.Include(r => r.Motel).Include(a => a.AppUser);
            return View(await motelAppContext.ToListAsync());
        }


        public async Task<IActionResult> MotelList()
        {
            return _context.Motels != null ?
                       View(await _context.Motels.ToListAsync()) :
                       Problem("Entity set 'MotelBookingAppContext.Airports'  is null.");
        }
         

        // GET: FavoriteMotel/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.FavoriteMotels == null)
            {
                return NotFound();
            }

            var favoriteMotel = await _context.FavoriteMotels.Include(m => m.Motel)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (favoriteMotel == null)
            {
                return NotFound();
            }

            return View(favoriteMotel);
        }

        [BindProperty]
        public int MotelId { get; set; }

        //[BindProperty]
        //public FavoriteMotel FavoriteMotel { get; set; }

        // GET: FavoriteMotel/Create
        public async Task<IActionResult> Create(int Id,FavoriteMotel favoriteMotel)

   
        {
            Motel Motel = await _context.Motels.FindAsync(Id);

            favoriteMotel.MotelId = Motel.Id;
            MotelId = Id;



            return View(favoriteMotel);
        }

        // POST: FavoriteMotel/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id")] FavoriteMotel favoriteMotel)
        {
       

            var userName = User.Identity.Name;
            var currentUser = _context.Users.Where(u => u.UserName == userName).FirstOrDefault();
            favoriteMotel.AppUser = currentUser;
            favoriteMotel.MotelId = MotelId;


            if (ModelState.IsValid)
            {
                _context.Add(favoriteMotel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            //ViewData["MotelId"] = new SelectList(_context.Motels, "MotelId", "MotelName", favoriteMotel.MotelId);
            return View(favoriteMotel);
        }

        // GET: FavoriteMotel/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.FavoriteMotels == null)
            {
                return NotFound();
            }

            var favoriteMotel = await _context.FavoriteMotels.Include(m =>m.Motel).AsNoTracking()
        .FirstOrDefaultAsync(m => m.Id == id);
            if (favoriteMotel == null)
            {
                return NotFound();
            }
            return View(favoriteMotel);
        }

        // POST: FavoriteMotel/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id")] FavoriteMotel favoriteMotel)
        {
            if (id != favoriteMotel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(favoriteMotel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FavoriteMotelExists(favoriteMotel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(favoriteMotel);
        }

        // GET: FavoriteMotel/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.FavoriteMotels == null)
            {
                return NotFound();
            }

            var favoriteMotel = await _context.FavoriteMotels
                .FirstOrDefaultAsync(m => m.Id == id);
            if (favoriteMotel == null)
            {
                return NotFound();
            }

            return View(favoriteMotel);
        }

        // POST: FavoriteMotel/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.FavoriteMotels == null)
            {
                return Problem("Entity set 'MotelDbContext.FavoriteMotel'  is null.");
            }
            var favoriteMotel = await _context.FavoriteMotels.FindAsync(id);
            if (favoriteMotel != null)
            {
                _context.FavoriteMotels.Remove(favoriteMotel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FavoriteMotelExists(int id)
        {
            return (_context.FavoriteMotels?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
