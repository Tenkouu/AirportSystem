using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using AirportSystem.Data;
using AirportSystem.Models;
using AirportSystem.Hubs;

namespace AirportSystem.Controllers
{
    /// <summary>
    /// API controller for handling passenger check-in operations in the airport system.
    /// Manages seat assignment and check-in status with real-time notifications.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CheckInController : ControllerBase
    {
        private readonly AirportDbContext _context;
        private readonly IHubContext<SeatHub> _hubContext;

        /// <summary>
        /// Initializes a new instance of the CheckInController class.
        /// </summary>
        /// <param name="context">The database context for check-in operations.</param>
        /// <param name="hubContext">The SignalR hub context for real-time notifications.</param>
        public CheckInController(AirportDbContext context, IHubContext<SeatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Processes passenger check-in with optional seat selection.
        /// Assigns a seat to the passenger and updates their check-in status.
        /// </summary>
        /// <param name="request">The check-in request containing passport number and optional seat selection.</param>
        /// <returns>Check-in response with passenger details and assigned seat information.</returns>
        [HttpPost]
        public async Task<ActionResult<CheckInResponse>> CheckInPassenger([FromBody] CheckInRequest request)
        {
            var passenger = await _context.Passengers
                .Include(p => p.Flight)
                .FirstOrDefaultAsync(p => p.PassportNumber == request.PassportNumber);

            if (passenger == null)
            {
                return NotFound(new { message = "Passenger not found with the provided passport number." });
            }

            if (passenger.IsCheckedIn)
            {
                return BadRequest(new { message = "Passenger is already checked in." });
            }

            Seat? selectedSeat = null;

            if (!string.IsNullOrEmpty(request.SelectedSeatNumber))
            {
                selectedSeat = await _context.Seats
                    .FirstOrDefaultAsync(s => s.FlightID == passenger.FlightID && 
                                            s.SeatNumber == request.SelectedSeatNumber);

                if (selectedSeat == null)
                {
                    return BadRequest(new { message = $"Seat {request.SelectedSeatNumber} not found for this flight." });
                }

                if (selectedSeat.IsOccupied)
                {
                    return BadRequest(new { message = $"Seat {request.SelectedSeatNumber} is already occupied." });
                }
            }
            else
            {
                selectedSeat = await _context.Seats
                    .FirstOrDefaultAsync(s => s.FlightID == passenger.FlightID && !s.IsOccupied);

                if (selectedSeat == null)
                {
                    return BadRequest(new { message = "No available seats for this flight." });
                }
            }

            passenger.AssignedSeatID = selectedSeat.SeatID;
            passenger.IsCheckedIn = true;
            selectedSeat.IsOccupied = true;
            selectedSeat.PassengerID = passenger.PassengerID;

            try
            {
                await _context.SaveChangesAsync();

                await _hubContext.Clients.Group($"Flight_{passenger.FlightID}")
                    .SendAsync("SeatOccupied", selectedSeat.SeatNumber);

                return Ok(new CheckInResponse
                {
                    Success = true,
                    Message = "Check-in successful",
                    PassengerName = passenger.FullName,
                    FlightNumber = passenger.Flight.FlightNumber,
                    SeatNumber = selectedSeat.SeatNumber,
                    Gate = passenger.Flight.Gate
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during check-in.", error = ex.Message });
            }
        }
    }

    /// <summary>
    /// Request model for passenger check-in operations.
    /// </summary>
    public class CheckInRequest
    {
        /// <summary>
        /// Gets or sets the passport number of the passenger checking in.
        /// </summary>
        public string PassportNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the preferred seat number for check-in.
        /// If not specified, the system will assign any available seat.
        /// </summary>
        public string? SelectedSeatNumber { get; set; }
    }

    /// <summary>
    /// Response model for passenger check-in operations.
    /// </summary>
    public class CheckInResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether the check-in was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the status message for the check-in operation.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the full name of the checked-in passenger.
        /// </summary>
        public string PassengerName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the flight number for the checked-in passenger.
        /// </summary>
        public string FlightNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the seat number assigned to the passenger.
        /// </summary>
        public string SeatNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the gate number for the passenger's flight.
        /// </summary>
        public string Gate { get; set; } = string.Empty;
    }
}
