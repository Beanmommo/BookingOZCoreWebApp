using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BookingOZCoreWebApp.Models
{
    public class Location
    {
        //Primary Key
        public int Id { get; set; }

        [Display(Name = "Location Name")]
        public string Name { get; set; }
        public string? Address { get; set; }
        public float? Lat { get; set; }
        public float? Long { get; set; }

        public ICollection<Booking> Bookings { get; set; }


    }

}
