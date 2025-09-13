using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using AirportSystem.Data;
using AirportSystem.Models;
using AirportSystem.Hubs;

namespace AirportSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckInController : ControllerBase
    {
        private readonly AirportDbContext _context;
        private readonly IHubContext<SeatHub> _hubContext;

        public CheckInController(AirportDbContext context, IHubContext<SeatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // POST: api/checkin
        [HttpPost]
        public async Task<ActionResult<CheckInResponse>> CheckInPassenger([FromBody] CheckInRequest request)
        {
            // Find passenger by passport number
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
                // Use the specifically selected seat
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
                // Fallback: Find any available seat for the passenger's flight
                selectedSeat = await _context.Seats
                    .FirstOrDefaultAsync(s => s.FlightID == passenger.FlightID && !s.IsOccupied);

                if (selectedSeat == null)
                {
                    return BadRequest(new { message = "No available seats for this flight." });
                }
            }

            // Assign seat to passenger
            passenger.AssignedSeatID = selectedSeat.SeatID;
            passenger.IsCheckedIn = true;
            selectedSeat.IsOccupied = true;
            selectedSeat.PassengerID = passenger.PassengerID;

            try
            {
                await _context.SaveChangesAsync();

                // Send SignalR notification to all clients in the flight group
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

    public class CheckInRequest
    {
        public string PassportNumber { get; set; } = string.Empty;
        public string? SelectedSeatNumber { get; set; }
    }

    public class CheckInResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string PassengerName { get; set; } = string.Empty;
        public string FlightNumber { get; set; } = string.Empty;
        public string SeatNumber { get; set; } = string.Empty;
        public string Gate { get; set; } = string.Empty;
    }
}
