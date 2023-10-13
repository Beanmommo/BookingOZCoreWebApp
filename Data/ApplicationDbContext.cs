using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BookingOZCoreWebApp.Models;

namespace BookingOZCoreWebApp.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<BookingOZCoreWebApp.Models.Booking>? Booking { get; set; }
        public DbSet<BookingOZCoreWebApp.Models.Location>? Location { get; set; }
        public DbSet<BookingOZCoreWebApp.Models.Report>? Report { get; set; }
    }
}