using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AirportSystem.Models
{
    public class Passenger
    {
        [Key]
        public int PassengerID { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string PassportNumber { get; set; } = string.Empty;

        [Required]
        public int FlightID { get; set; }

        public int? AssignedSeatID { get; set; }

        [Required]
        public bool IsCheckedIn { get; set; }

        // Navigation properties
        [ForeignKey("FlightID")]
        public virtual Flight Flight { get; set; } = null!;

        [ForeignKey("AssignedSeatID")]
        public virtual Seat? AssignedSeat { get; set; }
    }
}
