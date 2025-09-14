using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AirportSystem.Models
{
    /// <summary>
    /// Represents a flight in the airport management system.
    /// Contains flight details including route, timing, gate assignment, and current status.
    /// </summary>
    public class Flight
    {
        /// <summary>
        /// Gets or sets the unique identifier for the flight.
        /// </summary>
        [Key]
        public int FlightID { get; set; }

        /// <summary>
        /// Gets or sets the flight number (e.g., "EK201", "QF12").
        /// Maximum length is 10 characters.
        /// </summary>
        [Required]
        [StringLength(10)]
        public string FlightNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the departure airport code and name.
        /// Maximum length is 100 characters.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string ArrivalAirport { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the destination airport code and name.
        /// Maximum length is 100 characters.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string DestinationAirport { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the scheduled departure time for the flight.
        /// </summary>
        [Required]
        public DateTime Time { get; set; }

        /// <summary>
        /// Gets or sets the gate number where the flight will depart from.
        /// Maximum length is 10 characters.
        /// </summary>
        [Required]
        [StringLength(10)]
        public string Gate { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the current status of the flight.
        /// </summary>
        [Required]
        public FlightStatus FlightStatus { get; set; }

        /// <summary>
        /// Gets or sets the collection of seats available on this flight.
        /// </summary>
        public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();

        /// <summary>
        /// Gets or sets the collection of passengers booked on this flight.
        /// </summary>
        public virtual ICollection<Passenger> Passengers { get; set; } = new List<Passenger>();
    }
}
