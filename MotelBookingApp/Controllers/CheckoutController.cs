using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using MotelBookingApp.Models;
using Stripe;
using MotelBookingApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Identity;

namespace MotelBookingApp.Controllers
{
    //[Authorize]
    public class CheckoutController : Controller
    {
        private readonly MotelDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _config;

        [TempData]
        public string Amount { get; set; }

        public List<BookedRecord> PurchasedList;
        public List<BookingCart> PlannedList;

        public CheckoutController(MotelDbContext context, UserManager<AppUser> userManager, IConfiguration config)
        {
            this._context = context;
            this._userManager = userManager;
            _config = config;
        }
        public async Task<IActionResult> Index()
        {
            var userName = User.Identity.Name; // userName is email
            var user = _context.Users.Where(u => u.UserName == userName).FirstOrDefault();
            if (user != null)
            {
                Amount = _context.BookingCarts.Include("Room").Include("AppUser").Where(u => u.AppUser.UserName == userName).Sum(room => (double)room.Room.Price).ToString();
                ViewBag.Total = Amount;
            }
            ViewBag.Count = HttpContext.Session.GetString("count");
            return View();
        }

        [HttpPost]
        public ActionResult CreateCheckoutSession(string total)
        {
            ViewBag.Count = Convert.ToInt32(HttpContext.Session.GetString("count"));

            var userName = _userManager.GetUserName(User);
            PlannedList = _context.BookingCarts.Include("AppUser").Include("Room").Include("Room.Motel").Include("Room.RoomType").Where(u => u.AppUser.UserName == userName).ToList();
            List<SessionLineItemOptions> lineItems = new List<SessionLineItemOptions>();

            if (PlannedList.Count == 0)
            {
                TempData["CartOption"] = "Your cart is empty!";
                return RedirectToAction("Cart", "Home");
            }

            for (int i = 0; i < PlannedList.Count; i++)
            {
                // TimeSpan time difference
                var timeDiff = (PlannedList[i].CheckoutDate - PlannedList[i].CheckinDate);

                lineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = Convert.ToInt32(PlannedList[i].Room.Price) * 100 * timeDiff.Days,
                        Currency = "CAD",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"Motel: {PlannedList[i].Room.Motel.Name}; Room: {PlannedList[i].Room.RoomType.Name}",
                            Description = $"{PlannedList[i].CheckinDate.ToString("yyyy-MM-dd")} ~ {PlannedList[i].CheckoutDate.ToString("yyyy-MM-dd")}"
                        },
                    },
                    Quantity = 1,
                    TaxRates = new List<string> { "txr_1MgfAmExADfeSuiUs9uTYs9M", "txr_1MgfDDExADfeSuiUDkTbryGO" }
                });
            }

            var options = new SessionCreateOptions
            {
                LineItems = lineItems,         
                Mode = "payment",
                AllowPromotionCodes = true,

                SuccessUrl = "https://localhost:7267/Checkout/Success?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = "https://localhost:7267/Checkout/Cancel",
            };
            var service = new SessionService();
            Session session = service.Create(options);

            Response.Headers.Add("Location", session.Url);
            HttpContext.Session.SetString("count", "0");
            return new StatusCodeResult(303);
        }

        [HttpGet("/checkout/success")]
        public async Task<IActionResult> Success([FromQuery] string session_id)
        {
            var sessionService = new SessionService();
            Session session = sessionService.Get(session_id);

            var userName = _userManager.GetUserName(User);
            var user = _context.Users.Where(u => u.UserName == userName).FirstOrDefault();

            var purchaseConfirmationCode = session.PaymentIntentId;
            var whenPaid = DateTime.Now;
            var TotalAmount = Math.Round((decimal)session.AmountTotal / 100, 2);
            var newPurchase = new Booking { AppUser = user, ConfirmCode = purchaseConfirmationCode, PayTime = whenPaid, TotalAmount = TotalAmount };
            _context.Add(newPurchase);
            await _context.SaveChangesAsync();

            var booking = _context.Bookings.Include("AppUser").Where(p => p.ConfirmCode == purchaseConfirmationCode && p.AppUser.UserName == userName).FirstOrDefault();
            PlannedList = _context.BookingCarts.Include("AppUser").Include("Room").Where(u => u.AppUser.UserName == userName).ToList();

            foreach (var pr in PlannedList)
            {
                var newPurchasedRoom = new BookedRecord
                {
                    CheckinDate = pr.CheckinDate,
                    CheckoutDate = pr.CheckoutDate,
                    OccupantName = pr.AppUser.UserName,
                    Room = pr.Room,
                    Price = pr.Room.Price,
                    Booking = booking,
                };
                // save to purchased
                _context.BookedRecords.Add(newPurchasedRoom);
                await _context.SaveChangesAsync();

                // after save each field
                _context.BookingCarts.Remove(pr);
                await _context.SaveChangesAsync();
            }

            // Create a new invoice
            string filePath = GenerateInvoice(session_id);

            // Send the invoice
            SendInvoice(session.CustomerDetails.Email, filePath);

            ViewBag.AmountPaid = TotalAmount;
            ViewBag.Customer = userName;
            ViewBag.Email = session.CustomerDetails.Email;
            return View();
        }

        public ActionResult Cancel()
        {
            return View();
        }

        public void SendInvoice(string recipientEmail, string filePath)
        {
            using (var client = new SmtpClient())
            {
                client.Host = "smtp.gmail.com";
                client.Port = 587;
                client.EnableSsl = true;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;

                var credential = new System.Net.NetworkCredential
                {
                    UserName = _config["MyGmail"],  // replace with valid value
                    Password = _config["SMTP"]  // replace with valid value (SMTP generated password)
                };
                client.Credentials = credential;
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress("MotelBooking@gmail.com", "TravelBookingZGZ");
                mailMessage.To.Add(recipientEmail);
                mailMessage.Subject = "Invoice for your booking purchase";
                mailMessage.IsBodyHtml = true;
                mailMessage.Body = "Please find attached your invoice for your recent purchase.";
                mailMessage.Attachments.Add(new Attachment(filePath));

                client.Send(mailMessage);
            }
        }
        public string GenerateInvoice(string session_id)
        {
            var options = new SessionGetOptions { Expand = new List<string> { "line_items.data.price.product" } };
            var sessionService = new SessionService();
            Session session = sessionService.Get(session_id, options);

            //generate a pdf file of invoice
            string fileName = $"Invoice_{session_id.Substring(session_id.Length - 6).ToUpper()}.pdf";
            string filePath = @"..\" + fileName;
            using (var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                using (var document = new Document())
                {
                    var writer = PdfWriter.GetInstance(document, fs);

                    document.Open();

                    var font = FontFactory.GetFont(FontFactory.HELVETICA, 12);
                    var newLineParagraph = new Paragraph($"\n", font);

                    var bookingParagraph = new Paragraph($"Motel Booking") { Font = FontFactory.GetFont(FontFactory.HELVETICA, 20), Alignment = Element.ALIGN_CENTER };
                    document.Add(bookingParagraph);
                    document.Add(newLineParagraph);

                    var invoiceParagraph = new Paragraph($"Invoice Number:{session_id.Substring(session_id.Length - 6).ToUpper()}") { Font = FontFactory.GetFont(FontFactory.HELVETICA, 16) };
                    document.Add(invoiceParagraph);

                    var nameParagraph = new Paragraph($"Name: {_userManager.GetUserName(User).ToUpper()}") { Font = FontFactory.GetFont(FontFactory.HELVETICA, 16) };
                    document.Add(nameParagraph);

                    var dateParagraph = new Paragraph($"Date: {DateTime.Now.ToString("yyyy-MM-dd")}", font);
                    document.Add(dateParagraph);
                    document.Add(newLineParagraph);

                    var itemsTable = new PdfPTable(4) { DefaultCell = { Border = Rectangle.NO_BORDER } };
                    itemsTable.WidthPercentage = 100;
                    itemsTable.SetWidths(new float[] { 3, 3, 1, 1 });
                    itemsTable.AddCell(new PdfPCell(new Phrase("Room", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LIGHT_GRAY });
                    itemsTable.AddCell(new PdfPCell(new Phrase("CheckinDate - CheckoutDate", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LIGHT_GRAY });
                    itemsTable.AddCell(new PdfPCell(new Phrase("Quantity", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LIGHT_GRAY });
                    itemsTable.AddCell(new PdfPCell(new Phrase("Price(CAD)", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LIGHT_GRAY });

                    foreach (var item in session.LineItems)
                    {
                        itemsTable.AddCell(new PdfPCell(new Phrase(item.Price.Product.Name)));
                        itemsTable.AddCell(new PdfPCell(new Phrase(item.Price.Product.Description)) { HorizontalAlignment = Element.ALIGN_CENTER });
                        itemsTable.AddCell(new PdfPCell(new Phrase(item.Quantity.ToString())) { HorizontalAlignment = Element.ALIGN_RIGHT });
                        itemsTable.AddCell(new PdfPCell(new Phrase($"$ {Math.Round((decimal)item.AmountSubtotal / 100, 2)} ")) { HorizontalAlignment = Element.ALIGN_RIGHT });
                    }

                    document.Add(itemsTable);
                    document.Add(newLineParagraph);

                    var subtotalParagraph = new Paragraph($"Subtotal: ${Math.Round((decimal)session.AmountSubtotal / 100, 2)} {session.Currency.ToUpper()}", font) { Alignment = Element.ALIGN_RIGHT };
                    document.Add(subtotalParagraph);

                    var discountParagraph = new Paragraph($"Discount: ${Math.Round((decimal)session.TotalDetails.AmountDiscount / 100, 2)} {session.Currency.ToUpper()}", font) { Alignment = Element.ALIGN_RIGHT };
                    document.Add(discountParagraph);

                    var taxParagraph = new Paragraph($"Tax: ${Math.Round((decimal)session.TotalDetails.AmountTax / 100, 2)} {session.Currency.ToUpper()}", font) { Alignment = Element.ALIGN_RIGHT };
                    document.Add(taxParagraph);

                    var totalParagraph = new Paragraph($"Total: ${Math.Round((decimal)session.AmountTotal / 100, 2)} {session.Currency.ToUpper()}", font) { Alignment = Element.ALIGN_RIGHT };
                    document.Add(totalParagraph);
                    document.Add(newLineParagraph);

                    var thanksParagraph = new Paragraph($"Thank you for your purchase!\n") { Font = FontFactory.GetFont(FontFactory.HELVETICA, 20), Alignment = Element.ALIGN_CENTER };
                    document.Add(thanksParagraph);

                    document.Close();

                }
            }
            return filePath;

        }

    }


}
