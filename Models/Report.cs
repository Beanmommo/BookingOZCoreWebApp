using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingOZCoreWebApp.Models
{
    public class Report
    {
        public int Id { get; set; }
        public string Path { get; set; }
        [Display(Name = "Comments to Patient")]
        public string Description { get; set; }
        public int BookingId { get; set; }
        public Booking Booking { get; set; }
        [NotMapped]
        public string? ReportFile { get; set; }
    }
}
