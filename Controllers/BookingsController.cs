using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookingOZCoreWebApp.Data;
using BookingOZCoreWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Identity;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Text;

namespace BookingOZCoreWebApp.Controllers
{
    [Authorize]
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public BookingsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Bookings
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var booking = _context.Booking.Where(b => (b.PatientId == userId | b.StaffId == userId) & !b.IsFinalised);
            if (User.IsInRole("Admin"))
            {
                booking = _context.Booking.Where(b => !b.IsFinalised);
            }
            var applicationDbContext = booking.Include(b => b.Location)
                .Include(b => b.Staff)
                .Include(b => b.Patient);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Booking == null)
            {
                return NotFound();
            }

            var booking = await _context.Booking
                .Include(b => b.Location)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (booking == null)
            {
                return NotFound();
            }

            if (booking.IsFinalised)
            {
                var report = await _context.Report.FirstOrDefaultAsync(r => r.BookingId == booking.Id);
                var rating = await _context.Rating.FirstOrDefaultAsync(r => r.BookingId == booking.Id);
                
                ViewData["ReportFile"] = report.Path;
                ViewData["ReportComments"] = report.Description;
                ViewData["RatingGrade"] = rating?.RatingValue.ToString();
                ViewData["RatingComments"] = rating?.RatingDescription;
                if (ViewData["RatingGrade"] == null)
                {
                    ViewData["RatingGrade"] = "Patient has not provide any rating";
                }
                else
                {
                    ViewData["RatingGrade"] += " /10";
                }
                if (ViewData["RatingComments"] == null)
                {
                    ViewData["RatingComments"] = "Patient has not provide any comments";
                }
            }

            return View(booking);
        }
        // GET: Bookings/PreviousBooking
        public async Task<IActionResult> PreviousBooking()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var booking = _context.Booking.Where(b => (b.PatientId == userId | b.StaffId == userId) & b.IsFinalised);
            if (User.IsInRole("Admin"))
            {
                booking = _context.Booking.Where(b => !b.IsFinalised);
            }
            var applicationDbContext = booking.Include(b => b.Location)
                .Include(b => b.Staff)
                .Include(b => b.Patient);
            return View(await applicationDbContext.ToListAsync());
        }
        // GET: Bookings/Create
        public IActionResult Create()
        {
            ViewData["LocationId"] = new SelectList(_context.Set<Location>(), "Id", "Name");
            var StaffList = _userManager.GetUsersInRoleAsync("Staff").Result;
            ViewData["StaffId"] = new SelectList(StaffList, "Id", "UserName");
            return View();
        }

        // POST: Bookings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Date,StaffId,LocationId")] Booking booking)
        {
            booking.IsFinalised = false;
            booking.PatientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            booking.ServiceName = "X-Ray";
            booking.Location = await _context.Location.FirstOrDefaultAsync(m => m.Id == booking.LocationId);
            booking.Staff = await _context.Users.FirstOrDefaultAsync(m => m.Id == booking.StaffId);
            booking.Patient = await _context.Users.FirstOrDefaultAsync(m => m.Id == booking.PatientId);
            ModelState.Clear();
            TryValidateModel(booking);

            if (ModelState.IsValid)
            {
                _context.Add(booking);
                await SendCreationBookingEmail(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var message = string.Join(" | ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
                Console.Error.WriteLine(message);
                Console.WriteLine("MODEL NOT VALID LIAO");
            }
            ViewData["LocationId"] = new SelectList(_context.Set<Location>(), "Id", "Name", booking.LocationId);
            ViewData["StaffId"] = new SelectList(_context.Users, "Id", "UserName", booking.StaffId);
            return View(booking);
        }

        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Booking == null)
            {
                return NotFound();
            }

            var booking = await _context.Booking.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }
            ViewData["LocationId"] = new SelectList(_context.Set<Location>(), "Id", "Name", booking.LocationId);
            return View(booking);
        }

        // POST: Bookings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ServiceName,Date,PatientId,StaffId,LocationId")] Booking booking)
        {
            if (id != booking.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.Id))
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
            ViewData["LocationId"] = new SelectList(_context.Set<Location>(), "Id", "Name", booking.LocationId);
            return View(booking);
        }

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Booking == null)
            {
                return NotFound();
            }

            var booking = await _context.Booking
                .Include(b => b.Location)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Booking == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Booking'  is null.");
            }
            var booking = await _context.Booking.FindAsync(id);
            if (booking != null)
            {
                _context.Booking.Remove(booking);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
          return (_context.Booking?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private static async Task SendCreationBookingEmail(Booking booking)
        {
            var apikey = "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";
            var client = new SendGridClient(apikey);

            //Setup email
            var adminEmail = new EmailAddress("leonardo.prasetyo5@gmail.com", "Booking OZ");
            var patientEmail = new EmailAddress("lpra0002@student.monash.edu", booking.Patient.UserName);
            var staffEmail = new EmailAddress("lpra0002@student.monash.edu", booking.Staff.UserName);

            //Email content
            var Htmlcontent = GenerateBookingDetailsHtml(booking);
            var subjectPatient = $"{booking.Patient.UserName}: Booking Created";
            var subjectStaff = $"{booking.Staff.UserName}: Upcoming Booking Created";

            //Send email
            var patientMsg = MailHelper.CreateSingleEmail(adminEmail, patientEmail, subjectPatient, null, Htmlcontent);
            var staffMsg = MailHelper.CreateSingleEmail(adminEmail, staffEmail, subjectStaff, null ,Htmlcontent);
            var patientResponse = await client.SendEmailAsync(patientMsg);
            var staffResponse = await client.SendEmailAsync(staffMsg);
            
        }

        private static string GenerateBookingDetailsHtml(Booking booking)
        {
            //Email content
            var sb = new StringBuilder();
            sb.Append("<html><head><meta charset='utf-8'/>");
            sb.Append("<style>ul li{list-style:none;margin-top:3px;margin-bottom:3px;}");
            sb.Append("h1{font-size: 23px;margin-bottom: 0px;margin-top: 0px;");
            sb.Append("font-weight: normal;font-family: Arial;}");
            sb.Append("h2{margin: 3px;margin-left: 0px;font-size: 16px;}");
            sb.Append("h3{margin: 3px;font-size: 16px;}");
            sb.Append("h4{margin: 3px;margin-left: 0px;font-size: 13px;}");
            sb.Append("h5{margin: 3px;font-size: 11px;font-weight: normal;font-family: Arial;}");
            sb.Append("</style>");
            sb.Append("</head>");
            sb.Append("<body style='margin:auto;margin-top:0px;margin-bottom:5px;'>");
            sb.Append("<div>");
            sb.Append("<h2>Booking Details<h2>");
            sb.Append($"<h4>Booking Date:   {booking.Date.ToString()}</h4>");
            sb.Append($"<h4>Location Name:  {booking.Location.Name}</h4>");
            sb.Append($"<h4>Service Name:   {booking.ServiceName}</h4>");
            sb.Append($"<h4>Patient Name:   {booking.Patient.UserName}</h4>");
            sb.Append($"<h4>Assigned Staff: {booking.Staff.UserName}</h4>");
            sb.Append("</div>");
            sb.Append("</body>");
            sb.Append("</html>");
            return sb.ToString();
        }
    }
}
