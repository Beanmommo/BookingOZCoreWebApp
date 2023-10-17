using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookingOZCoreWebApp.Data;
using BookingOZCoreWebApp.Models;
using SendGrid.Helpers.Mail;
using SendGrid;
using System.Text;
using System.Net.Mail;

namespace BookingOZCoreWebApp.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ReportsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Reports
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Report.Include(r => r.Booking);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Reports/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Report == null)
            {
                return NotFound();
            }

            var report = await _context.Report
                .Include(r => r.Booking)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        // GET: Reports/Create
        public IActionResult Create(int bookingId)
        {
            if (_context.Report == null)
            {
                return NotFound();
            }
            ViewBag.BookingId = bookingId;
            //ViewData["BookingId"] = new SelectList(_context.Booking, "Id", "Id");
            var report = new Report();
            report.BookingId = bookingId;
            Console.WriteLine(report.BookingId);
            return View(report);
        }

        // POST: Reports/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ReportFile,Description,BookingId")] Report report, IFormFile ReportFile)
        {
            ModelState.Clear();
            var myUniqueFileName = string.Format(@"{0}", Guid.NewGuid());
            report.Path = myUniqueFileName;
            Console.WriteLine(report.BookingId.ToString());
            var booking = await _context.Booking.FirstOrDefaultAsync(m => m.Id == report.BookingId);
            booking.Location = await _context.Location.FirstOrDefaultAsync(m => m.Id == booking.LocationId);
            booking.Staff = await _context.Users.FirstOrDefaultAsync(m => m.Id == booking.StaffId);
            booking.Patient = await _context.Users.FirstOrDefaultAsync(m => m.Id == booking.PatientId);
            report.Booking = booking;

            //Finalised booking
            booking.IsFinalised = true;

            TryValidateModel(booking);
            TryValidateModel(report);

            if (ModelState.IsValid)
            {
               // var path = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", myUniqueFileName);
                
                string serverPath = _webHostEnvironment.ContentRootPath;
                string uploadPath = Path.Combine(serverPath, "Uploads/");
                string fileExtenstion = Path.GetExtension(ReportFile.FileName);
                string filePath = report.Path + fileExtenstion;
                report.Path = filePath;
                string completePath = uploadPath + report.Path;
                using (System.IO.Stream stream = new FileStream(completePath, FileMode.Create))
                
                {
                    await ReportFile.CopyToAsync(stream);
                }
                //Send email
                await SendBookingReportEmail(report.Booking, report, completePath);
                //Write to database
                _context.Add(report);
                _context.Update(booking);
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
            
            ViewData["BookingId"] = report.BookingId;

            //return RedirectToAction(nameof(Create), new {bookingId =  report.BookingId});
            return View(report);
        }

        // GET: Reports/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Report == null)
            {
                return NotFound();
            }

            var report = await _context.Report.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }
            ViewData["BookingId"] = new SelectList(_context.Booking, "Id", "Id", report.BookingId);
            return View(report);
        }

        // POST: Reports/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Path,Description,BookingId")] Report report)
        {
            if (id != report.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(report);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReportExists(report.Id))
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
            ViewData["BookingId"] = new SelectList(_context.Booking, "Id", "Id", report.BookingId);
            return View(report);
        }

        // GET: Reports/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Report == null)
            {
                return NotFound();
            }

            var report = await _context.Report
                .Include(r => r.Booking)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        // POST: Reports/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Report == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Report'  is null.");
            }
            var report = await _context.Report.FindAsync(id);
            if (report != null)
            {
                _context.Report.Remove(report);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReportExists(int id)
        {
          return (_context.Report?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private static async Task SendBookingReportEmail(Booking booking, Report report, string reportFilePath)
        {
            var apikey = Environment.GetEnvironmentVariable("BookingOzSendGrid");
            var client = new SendGridClient(apikey);

            //Setup email
            var adminEmail = new EmailAddress("leonardo.prasetyo5@gmail.com", "Booking OZ");
            var patientEmail = new EmailAddress("lpra0002@student.monash.edu", booking.Patient.UserName);
            var staffEmail = new EmailAddress("lpra0002@student.monash.edu", booking.Staff.UserName);

            //Email content
            var Htmlcontent = GenerateBookingDetailsHtml(booking, report);
            var subjectPatient = $"{booking.Patient.UserName}: Medical Imaging Report";
            var subjectStaff = $"{booking.Staff.UserName}: Medical Imaging Report Uploaded";

            //Send email
            var patientMsg = MailHelper.CreateSingleEmail(adminEmail, patientEmail, subjectPatient, null, Htmlcontent);
            var staffMsg = MailHelper.CreateSingleEmail(adminEmail, staffEmail, subjectStaff, null, Htmlcontent);

            //Add email attatchment
            byte[] reportContent = System.IO.File.ReadAllBytes(reportFilePath);
            
            patientMsg.AddAttachment(report.Path, Convert.ToBase64String(reportContent));
            staffMsg.AddAttachment(report.Path, Convert.ToBase64String(reportContent));
            var patientResponse = await client.SendEmailAsync(patientMsg);
            var staffResponse = await client.SendEmailAsync(staffMsg);
            Console.WriteLine(patientResponse.StatusCode);

        }


        private static string GenerateBookingDetailsHtml(Booking booking, Report report)
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
            sb.Append("<h2>Staff Comments<h2>");
            sb.Append($"<h4>{report.Description}</h4>");
            sb.Append("</div>");
            sb.Append("</body>");
            sb.Append("</html>");
            return sb.ToString();
        }
    }
}
