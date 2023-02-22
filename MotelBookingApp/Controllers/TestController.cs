using MotelBookingApp.Data;
using MotelBookingApp.Models;
using MotelBookingApp.Data.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using Microsoft.AspNetCore.Authorization;

namespace MotelBookingApp.Controllers
{
    public class TestController : Controller
    {
        private readonly MotelDbContext _context;
        private readonly IConfiguration _config;

        public TestController(MotelDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(BugSubmitVM bugSubmitVM)
        {
            if (!ModelState.IsValid) return View(bugSubmitVM);

            var newBug = new BugSubmitVM();
            newBug.Name = bugSubmitVM.Name;
            newBug.Comment = bugSubmitVM.Comment;

            var body = $@"<p>Bug Sumbit Info:</p>
                          <p><b>Name:</b> {newBug.Name}</p>
                          <p><b>Comment:</b><br/>{newBug.Comment}</p>";

            // send confirmation email(GMAIL SMTP)
            using (var smtp = new SmtpClient())
            {
                var message = new MailMessage();
                var credential = new System.Net.NetworkCredential
                {
                    UserName = _config["MyGmail"],  // replace with valid value
                    Password = _config["SMTP"]  // replace with valid value (SMTP generated password)
                };
                smtp.Credentials = credential;
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                message.To.Add(credential.UserName); // replace with registered email (newUser.Email)
                message.Subject = $"Bug Submission from {newBug.Name}";
                message.Body = body;
                message.IsBodyHtml = true;
                message.From = new MailAddress("example@gmail.com");
                await smtp.SendMailAsync(message);
            }
            TempData["BugSumbit"] = "Thank you for your submission!";
            return RedirectToAction("Index", "Test");
        }
    }
}


