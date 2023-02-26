//using MotelBookingApp.Data;
//using MotelBookingApp.Models;
//using MotelBookingApp.Data.ViewModels;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System.Net.Mail;
//using Microsoft.AspNetCore.Authorization;

//namespace MotelBookingApp.Controllers
//{
//    public class AccountController : Controller
//    {
//        private readonly MotelDbContext _context;
//        private readonly UserManager<AppUser> _userManager;
//        private readonly SignInManager<AppUser> _signInManager;
//        private readonly IConfiguration _config;
//        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, MotelDbContext context, IConfiguration config)
//        {
//            this._userManager = userManager;
//            this._signInManager = signInManager;
//            _context = context;
//            _config = config;
//        }

        //[HttpGet]
        //[AllowAnonymous]
        //public async Task<IActionResult> Login() => View(new LoginVM());

//        [HttpPost]
//        [AllowAnonymous]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Login(LoginVM loginVM, string returnUrl = null)
//        {
//            if (!ModelState.IsValid) return View(loginVM);

//            var user = await _userManager.FindByEmailAsync(loginVM.Email);
//            if (user != null)
//            {
//                if (user.EmailConfirmed == true)
//                {
//                    var passwordCheck = await _userManager.CheckPasswordAsync(user, loginVM.Password);
//                    if (passwordCheck)
//                    {
//                        var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, false, false);
//                        if (result.Succeeded)
//                        {
//                            return RedirectToAction("Index", "Home");
//                        }
//                    }
//                    TempData["LoginError"] = "Wrong credentials. Please, try again!";
//                    return View(loginVM);
//                }
//                TempData["LoginError"] = "Please go to your email address to confirm first.";
//                return View(loginVM);
//            }
//            TempData["LoginError"] = "Wrong credentials. Please, try again!";
//            return View(loginVM);
//        }

//        [HttpGet]
//        [AllowAnonymous]
//        public IActionResult Register() => View(new RegisterVM());

//        [HttpPost]
//        [AllowAnonymous]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Register(RegisterVM registerVM, string returnUrl = null)
//        {
//            ViewData["ReturnUrl"] = returnUrl;
//            if (!ModelState.IsValid) return View(registerVM);

//            var user = await _userManager.FindByEmailAsync(registerVM.Email);
//            if (user != null)
//            {
//                TempData["Error"] = "This email address is already in use";
//                return View(registerVM);
//            }

//            var newUser = new AppUser()
//            {
//                Email = registerVM.Email,
//                UserName = registerVM.UserName,
//                PhoneNumber = registerVM.PhoneNumber,
//                FirstName = registerVM.FirstName,
//                LastName = registerVM.LastName,
//                DOB = registerVM.DOB,
//            };
//            var newUserResponse = await _userManager.CreateAsync(newUser, registerVM.Password);

//            if (newUserResponse.Succeeded)
//            {
//                // sign user to "User"
//                var signToUser = await _userManager.AddToRoleAsync(newUser, "User");

//                /* ****************** TEST USE  ***************** */
//                var code = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
//                var result = await _userManager.ConfirmEmailAsync(newUser, code);

//                if (result.Succeeded)
//                {
//                    TempData["LoginFlash"] = "Registration Successfully, Please Login!";
//                    return RedirectToAction("Login", "Account");
//                }
//                /* **************** TEST USE END **************** */

//                //if (signToUser.Succeeded)
//                //{
//                //    var code = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
//                //    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = newUser.Id, code = code }, protocol: HttpContext.Request.Scheme);

//                //    var body = $@"<p>Thank you for registering our booking system!</p>
//                //            <p>Your Username is: <br/>{newUser.UserName}</p>
//                //            <p>Please use your Email {newUser.Email} to Login.<br/></p>
//                //            <a href='{callbackUrl}'>
//                //            Please click here to confirm your email</a>";

//                //    // send confirmation email(GMAIL SMTP)
//                //    using (var smtp = new SmtpClient())
//                //    {
//                //        var message = new MailMessage();
//                //        var credential = new System.Net.NetworkCredential
//                //        {
//                //            UserName = _config["MyGmail"],  // replace with valid value
//                //            Password = _config["SMTP"]  // replace with valid value (SMTP generated password)
//                //        };
//                //        smtp.Credentials = credential;
//                //        smtp.Host = "smtp.gmail.com";
//                //        smtp.Port = 587;
//                //        smtp.EnableSsl = true;
//                //        message.To.Add(newUser.UserName); // replace with registered email (newUser.Email)
//                //        message.Subject = "Your Booking from Motel Booking System";
//                //        message.Body = body;
//                //        message.IsBodyHtml = true;
//                //        message.From = new MailAddress("example@gmail.com");
//                //        await smtp.SendMailAsync(message);
//                //    }
//                //    return View("RegisterCompleted");
//                //}
//                else
//                {
//                    //FIXME: delete the user since role assignment failed, log the event, show error to user
//                }
//            }
//            foreach (var error in newUserResponse.Errors)
//            {
//                ModelState.AddModelError(string.Empty, error.Description);
//            }
//            return View(registerVM);

//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Logout()
//        {
//            await _signInManager.SignOutAsync();
//            TempData["LoginFlash"] = "Logged out";
//            return RedirectToAction("Login", "Account");
//        }

//        public IActionResult AccessDenied(string ReturnUrl)
//        {
//            return View();
//        }


//        [HttpGet("ConfirmEmail")]
//        [AllowAnonymous]
//        public async Task<IActionResult> ConfirmEmail(string userId, string code)
//        {
//            var user = await _userManager.FindByIdAsync(userId);
//            if (user == null)
//            {
//                return NotFound();
//            }

//            var result = await _userManager.ConfirmEmailAsync(user, code);
//            if (result.Succeeded)
//            {
//                TempData["LoginFlash"] = "Email Confirmed!";
//                return RedirectToAction("Login", "Account");
//            }
//            TempData["LoginFlash"] = "Email confirm failed!";
//            return RedirectToAction("Login", "Account");
//        }

//        public IActionResult ForgotPassword()
//        {
//            return View();
//        }

//        [HttpPost]
//        [AllowAnonymous]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
//        {
//            if (ModelState.IsValid)
//            {
//                var user = await _userManager.FindByEmailAsync(model.Email);
//                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
//                {
//                    TempData["LoginError"] = "Wrong credentials. Please, try again!";
//                    return RedirectToAction("Login", "Account");
//                }

//                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
//                var callbackUrl = Url.Action("ResetPassword", "Account", new { code = code }, protocol: HttpContext.Request.Scheme);

//                var body = $@"<p>Thank you for visiting our booking system!</p>
//                            <p>Your Username is: <br/>{user.UserName}</p>
//                            <br/>
//                            <a href='{callbackUrl}'>Please click HERE to reset your password.</a>";

//                // send confirmation email(GMAIL SMTP)
//                using (var smtp = new SmtpClient())
//                {
//                    var message = new MailMessage();
//                    var credential = new System.Net.NetworkCredential
//                    {
//                        UserName = _config["MyGmail"],  // replace with valid value
//                        Password = _config["SMTP"]  // replace with valid value (SMTP generated password)
//                    };
//                    smtp.Credentials = credential;
//                    smtp.Host = "smtp.gmail.com";
//                    smtp.Port = 587;
//                    smtp.EnableSsl = true;
//                    message.To.Add(credential.UserName); // replace with registered email (newUser.Email)
//                    message.Subject = "Reset Password - Motel Booking System";
//                    message.Body = body;
//                    message.IsBodyHtml = true;
//                    message.From = new MailAddress("example@gmail.com");
//                    await smtp.SendMailAsync(message);
//                }
//                TempData["LoginFlash"] = "Reset Password email sent!";
//                return RedirectToAction("Login", "Account");
//            }
//            return View(model);
//        }

//        [HttpGet]
//        [AllowAnonymous]
//        public IActionResult ResetPassword(string code = null)
//        {
//            var model = new ResetPasswordViewModel { Code = code };
//            return View(model);
//        }

//        [HttpPost]
//        [AllowAnonymous]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
//        {
//            if (!ModelState.IsValid)
//            {
//                return View(model);
//            }
//            var user = await _userManager.FindByEmailAsync(model.Email);
//            if (user == null)
//            {
//                TempData["ResetPassError"] = "Wrong email. Please try again!";
//                return RedirectToAction("ResetPassword", "Account", new { code = model.Code });
//            }
//            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
//            if (result.Succeeded)
//            {
//                TempData["LoginFlash"] = "Reset Password Succeeded!";
//                return RedirectToAction("Login", "Account");
//            }
//            TempData["ResetPassError"] = "Reset Failed!";
//            return View();
//        }

//        [HttpGet]
//        public IActionResult AccessDenied()
//        {
//            return View();
//        }
//    }
//}
