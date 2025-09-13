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
    public class SeatsController : ControllerBase
    {
        private readonly AirportDbContext _context;
        private readonly IHubContext<SeatHub> _hubContext;

        public SeatsController(AirportDbContext context, IHubContext<SeatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // GET: api/seats
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Seat>>> GetSeats()
        {
            return await _context.Seats
                .Include(s => s.Flight)
                .Include(s => s.Passenger)
                .ToListAsync();
        }

        // GET: api/seats/5
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

        // GET: api/seats/flight/5
        [HttpGet("flight/{flightId}")]
        public async Task<ActionResult<IEnumerable<Seat>>> GetSeatsByFlight(int flightId)
        {
            var seats = await _context.Seats
                .Include(s => s.Passenger)
                .Where(s => s.FlightID == flightId)
                .ToListAsync();

            return seats;
        }

        // POST: api/seats
        [HttpPost]
        public async Task<ActionResult<Seat>> PostSeat(Seat seat)
        {
            _context.Seats.Add(seat);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSeat", new { id = seat.SeatID }, seat);
        }

        // PUT: api/seats/5
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

        // DELETE: api/seats/5
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

        // POST: api/seats/occupy
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

                // Send SignalR notification to all clients in the flight group
                await _hubContext.Clients.Group($"Flight_{seat.FlightID}")
                    .SendAsync("SeatOccupied", seat.SeatNumber);

                return Ok(new { message = "Seat occupied successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while occupying the seat.", error = ex.Message });
            }
        }

        // POST: api/seats/release
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

                // Send SignalR notification to all clients in the flight group
                await _hubContext.Clients.Group($"Flight_{seat.FlightID}")
                    .SendAsync("SeatAvailable", seat.SeatNumber);

                return Ok(new { message = "Seat released successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while releasing the seat.", error = ex.Message });
            }
        }

        private bool SeatExists(int id)
        {
            return _context.Seats.Any(e => e.SeatID == id);
        }
    }

    public class OccupySeatRequest
    {
        public int SeatId { get; set; }
        public int? PassengerId { get; set; }
    }

    public class ReleaseSeatRequest
    {
        public int SeatId { get; set; }
    }
}
