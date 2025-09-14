using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AirportSystem.Models
{
    /// <summary>
    /// Represents a seat on a flight in the airport management system.
    /// Contains seat identification, occupancy status, and passenger assignment.
    /// </summary>
    public class Seat
    {
        /// <summary>
        /// Gets or sets the unique identifier for the seat.
        /// </summary>
        [Key]
        public int SeatID { get; set; }

        /// <summary>
        /// Gets or sets the ID of the flight this seat belongs to.
        /// </summary>
        [Required]
        public int FlightID { get; set; }

        /// <summary>
        /// Gets or sets the seat number (e.g., "1A", "2B").
        /// Maximum length is 10 characters.
        /// </summary>
        [Required]
        [StringLength(10)]
        public string SeatNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the seat is currently occupied.
        /// </summary>
        [Required]
        public bool IsOccupied { get; set; }

        /// <summary>
        /// Gets or sets the ID of the passenger assigned to this seat.
        /// Null if the seat is not occupied.
        /// </summary>
        public int? PassengerID { get; set; }

        /// <summary>
        /// Gets or sets the flight this seat belongs to.
        /// </summary>
        [ForeignKey("FlightID")]
        public virtual Flight Flight { get; set; } = null!;

        /// <summary>
        /// Gets or sets the passenger assigned to this seat.
        /// Null if the seat is not occupied.
        /// </summary>
        [ForeignKey("PassengerID")]
        public virtual Passenger? Passenger { get; set; }
    }
}
