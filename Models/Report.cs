using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BookingOZCoreWebApp.Models
{
    public class Report
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public string Description { get; set; }
        public int BookingId { get; set; }
        public Booking Booking { get; set; }
    }
}
