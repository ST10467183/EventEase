using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Venue
    {
        [Key]
        public int VenueId { get; set; }

        [Required(ErrorMessage = "Venue name is required")]
        [Display(Name = "Venue Name")]
        public string VenueName { get; set; }

        [Required(ErrorMessage = "Location is required")]
        public string Location { get; set; }

        [Range(1, 100000, ErrorMessage = "Capacity must be at least 1")]
        public int Capacity { get; set; }

        [Display(Name = "Image URL")]
        [DataType(DataType.ImageUrl)]
        public string? ImageUrl { get; set; }

        public List<Booking>? Bookings { get; set; }
    }
}