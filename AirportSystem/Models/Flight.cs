using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AirportSystem.Models
{
    public class Flight
    {
        [Key]
        public int FlightID { get; set; }

        [Required]
        [StringLength(10)]
        public string FlightNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ArrivalAirport { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string DestinationAirport { get; set; } = string.Empty;

        [Required]
        public DateTime Time { get; set; }

        [Required]
        [StringLength(10)]
        public string Gate { get; set; } = string.Empty;

        [Required]
        public FlightStatus FlightStatus { get; set; }

        // Navigation properties
        public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();
        public virtual ICollection<Passenger> Passengers { get; set; } = new List<Passenger>();
    }
}
