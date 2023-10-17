using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BookingOZCoreWebApp.Models
{
    public class Booking
    {

        public int Id { get; set; }
        public string ServiceName { get; set; }
        public DateTime Date { get; set; }
        public bool IsFinalised { get; set; }
        public string PatientId { get; set; }
        public string StaffId { get; set; }
        public int LocationId { get; set; }
        public Location Location { get; set; }
        public IdentityUser Patient { get; set; }
        public IdentityUser Staff{ get; set; }
        
    }
}
