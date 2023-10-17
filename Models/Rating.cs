namespace BookingOZCoreWebApp.Models
{
    public class Rating
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public int RatingValue { get; set; }
        public string? RatingDescription { get; set; }
        public Booking Booking { get; set; }
    }
}
