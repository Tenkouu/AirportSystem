using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using AirportSystem.Data;
using AirportSystem.Models;
using AirportSystem.Hubs;

namespace AirportSystem.Controllers
{
    /// <summary>
    /// API controller for managing seats in the airport system.
    /// Provides CRUD operations for seat data and real-time occupancy updates via SignalR.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SeatsController : ControllerBase
    {
        private readonly AirportDbContext _context;
        private readonly IHubContext<SeatHub> _hubContext;

        /// <summary>
        /// Initializes a new instance of the SeatsController class.
        /// </summary>
        /// <param name="context">The database context for seat operations.</param>
        /// <param name="hubContext">The SignalR hub context for real-time notifications.</param>
        public SeatsController(AirportDbContext context, IHubContext<SeatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Retrieves all seats with their associated flight and passenger information.
        /// </summary>
        /// <returns>A collection of all seats in the system.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Seat>>> GetSeats()
        {
            return await _context.Seats
                .Include(s => s.Flight)
                .Include(s => s.Passenger)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a specific seat by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the seat.</param>
        /// <returns>The seat with the specified ID, or NotFound if not found.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Seat>> GetSeat(int id)
        {
            var seat = await _context.Seats
                .Include(s => s.Flight)
                .Include(s => s.Passenger)
                .FirstOrDefaultAsync(s => s.SeatID == id);

            if (seat == null)
            {
                return NotFound();
            }

            return seat;
        }

        /// <summary>
        /// Retrieves all seats for a specific flight.
        /// </summary>
        /// <param name="flightId">The ID of the flight to get seats for.</param>
        /// <returns>A collection of seats for the specified flight.</returns>
        [HttpGet("flight/{flightId}")]
        public async Task<ActionResult<IEnumerable<Seat>>> GetSeatsByFlight(int flightId)
        {
            var seats = await _context.Seats
                .Include(s => s.Passenger)
                .Where(s => s.FlightID == flightId)
                .ToListAsync();

            return seats;
        }

        /// <summary>
        /// Creates a new seat in the system.
        /// </summary>
        /// <param name="seat">The seat object to create.</param>
        /// <returns>The created seat with its assigned ID.</returns>
        [HttpPost]
        public async Task<ActionResult<Seat>> PostSeat(Seat seat)
        {
            _context.Seats.Add(seat);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSeat", new { id = seat.SeatID }, seat);
        }

        /// <summary>
        /// Updates an existing seat.
        /// </summary>
        /// <param name="id">The ID of the seat to update.</param>
        /// <param name="seat">The updated seat data.</param>
        /// <returns>NoContent if successful, BadRequest if IDs don't match, NotFound if seat doesn't exist.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSeat(int id, Seat seat)
        {
            if (id != seat.SeatID)
            {
                return BadRequest();
            }

            _context.Entry(seat).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SeatExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Deletes a seat from the system.
        /// </summary>
        /// <param name="id">The ID of the seat to delete.</param>
        /// <returns>NoContent if successful, NotFound if seat doesn't exist.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSeat(int id)
        {
            var seat = await _context.Seats.FindAsync(id);
            if (seat == null)
            {
                return NotFound();
            }

            _context.Seats.Remove(seat);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Occupies a seat and assigns it to a passenger.
        /// Sends real-time notification to all clients in the flight group.
        /// </summary>
        /// <param name="request">The seat occupation request containing seat and passenger IDs.</param>
        /// <returns>Success message if occupied, error message if seat not found or already occupied.</returns>
        [HttpPost("occupy")]
        public async Task<IActionResult> OccupySeat([FromBody] OccupySeatRequest request)
        {
            var seat = await _context.Seats
                .Include(s => s.Flight)
                .FirstOrDefaultAsync(s => s.SeatID == request.SeatId);

            if (seat == null)
            {
                return NotFound(new { message = "Seat not found." });
            }

            if (seat.IsOccupied)
            {
                return BadRequest(new { message = "Seat is already occupied." });
            }

            seat.IsOccupied = true;
            seat.PassengerID = request.PassengerId;

            try
            {
                await _context.SaveChangesAsync();

                await _hubContext.Clients.Group($"Flight_{seat.FlightID}")
                    .SendAsync("SeatOccupied", seat.SeatNumber);

                return Ok(new { message = "Seat occupied successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while occupying the seat.", error = ex.Message });
            }
        }

        /// <summary>
        /// Releases a seat, making it available for other passengers.
        /// Sends real-time notification to all clients in the flight group.
        /// </summary>
        /// <param name="request">The seat release request containing the seat ID.</param>
        /// <returns>Success message if released, error message if seat not found or not occupied.</returns>
        [HttpPost("release")]
        public async Task<IActionResult> ReleaseSeat([FromBody] ReleaseSeatRequest request)
        {
            var seat = await _context.Seats
                .Include(s => s.Flight)
                .FirstOrDefaultAsync(s => s.SeatID == request.SeatId);

            if (seat == null)
            {
                return NotFound(new { message = "Seat not found." });
            }

            if (!seat.IsOccupied)
            {
                return BadRequest(new { message = "Seat is not occupied." });
            }

            seat.IsOccupied = false;
            seat.PassengerID = null;

            try
            {
                await _context.SaveChangesAsync();

                await _hubContext.Clients.Group($"Flight_{seat.FlightID}")
                    .SendAsync("SeatAvailable", seat.SeatNumber);

                return Ok(new { message = "Seat released successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while releasing the seat.", error = ex.Message });
            }
        }

        /// <summary>
        /// Checks if a seat with the specified ID exists in the database.
        /// </summary>
        /// <param name="id">The seat ID to check.</param>
        /// <returns>True if the seat exists, false otherwise.</returns>
        private bool SeatExists(int id)
        {
            return _context.Seats.Any(e => e.SeatID == id);
        }
    }

    /// <summary>
    /// Request model for occupying a seat.
    /// </summary>
    public class OccupySeatRequest
    {
        /// <summary>
        /// Gets or sets the ID of the seat to occupy.
        /// </summary>
        public int SeatId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the passenger to assign to the seat.
        /// </summary>
        public int? PassengerId { get; set; }
    }

    /// <summary>
    /// Request model for releasing a seat.
    /// </summary>
    public class ReleaseSeatRequest
    {
        /// <summary>
        /// Gets or sets the ID of the seat to release.
        /// </summary>
        public int SeatId { get; set; }
    }
}
