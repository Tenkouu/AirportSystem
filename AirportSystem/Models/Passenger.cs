using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AirportSystem.Models
{
    /// <summary>
    /// Represents a passenger in the airport management system.
    /// Contains passenger information, flight association, seat assignment, and check-in status.
    /// </summary>
    public class Passenger
    {
        /// <summary>
        /// Gets or sets the unique identifier for the passenger.
        /// </summary>
        [Key]
        public int PassengerID { get; set; }

        /// <summary>
        /// Gets or sets the full name of the passenger.
        /// Maximum length is 100 characters.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the passport number of the passenger.
        /// Must be unique across all passengers. Maximum length is 20 characters.
        /// </summary>
        [Required]
        [StringLength(20)]
        public string PassportNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ID of the flight this passenger is booked on.
        /// </summary>
        [Required]
        public int FlightID { get; set; }

        /// <summary>
        /// Gets or sets the ID of the seat assigned to this passenger.
        /// Null if no seat has been assigned yet.
        /// </summary>
        public int? AssignedSeatID { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the passenger has checked in.
        /// </summary>
        [Required]
        public bool IsCheckedIn { get; set; }

        /// <summary>
        /// Gets or sets the flight this passenger is booked on.
        /// </summary>
        [ForeignKey("FlightID")]
        public virtual Flight Flight { get; set; } = null!;

        /// <summary>
        /// Gets or sets the seat assigned to this passenger.
        /// Null if no seat has been assigned.
        /// </summary>
        [ForeignKey("AssignedSeatID")]
        public virtual Seat? AssignedSeat { get; set; }
    }
}
