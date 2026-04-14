using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [Required(ErrorMessage = "Venue is required")]
        [Display(Name = "Venue")]
        public int VenueId { get; set; }

        [Required(ErrorMessage = "Event is required")]
        [Display(Name = "Event")]
        public int EventId { get; set; }

        [Required]
        [Display(Name = "Booking Date")]
        [DataType(DataType.DateTime)]
        public DateTime BookingDate { get; set; }

        [ForeignKey("VenueId")]
        public Venue? Venue { get; set; }

        [ForeignKey("EventId")]
        public Event? Event { get; set; }
    }
}
