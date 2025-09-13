using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AirportSystem.Models
{
    public class Seat
    {
        [Key]
        public int SeatID { get; set; }

        [Required]
        public int FlightID { get; set; }

        [Required]
        [StringLength(10)]
        public string SeatNumber { get; set; } = string.Empty;

        [Required]
        public bool IsOccupied { get; set; }

        public int? PassengerID { get; set; }

        // Navigation properties
        [ForeignKey("FlightID")]
        public virtual Flight Flight { get; set; } = null!;

        [ForeignKey("PassengerID")]
        public virtual Passenger? Passenger { get; set; }
    }
}
