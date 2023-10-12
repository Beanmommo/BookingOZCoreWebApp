namespace BookingOZCoreWebApp.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int CustomerId { get; set; }
        public int StaffId { get; set; }
        public int LocationId { get; set; }
    }
}
