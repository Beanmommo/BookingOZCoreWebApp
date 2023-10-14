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
                Console.WriteLine(completePath);

                _context.Add(report);
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
    }
}
