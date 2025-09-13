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
    public class FlightsController : ControllerBase
    {
        private readonly AirportDbContext _context;
        private readonly IHubContext<FlightHub> _hubContext;

        public FlightsController(AirportDbContext context, IHubContext<FlightHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // GET: api/flights
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Flight>>> GetFlights()
        {
            return await _context.Flights
                .Include(f => f.Seats)
                .Include(f => f.Passengers)
                .ToListAsync();
        }

        // GET: api/flights/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Flight>> GetFlight(int id)
        {
            var flight = await _context.Flights
                .Include(f => f.Seats)
                .Include(f => f.Passengers)
                .FirstOrDefaultAsync(f => f.FlightID == id);

            if (flight == null)
            {
                return NotFound();
            }

            return flight;
        }

        // POST: api/flights
        [HttpPost]
        public async Task<ActionResult<Flight>> PostFlight(Flight flight)
        {
            _context.Flights.Add(flight);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetFlight", new { id = flight.FlightID }, flight);
        }

        // PUT: api/flights/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFlight(int id, Flight flight)
        {
            if (id != flight.FlightID)
            {
                return BadRequest();
            }

            _context.Entry(flight).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FlightExists(id))
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

        // PUT: api/flights/{id}/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateFlightStatus(int id, [FromBody] FlightStatusUpdateRequest request)
        {
            var flight = await _context.Flights.FindAsync(id);
            if (flight == null)
            {
                return NotFound();
            }

            // Parse string status to enum
            if (Enum.TryParse<FlightStatus>(request.FlightStatus, true, out var status))
            {
                flight.FlightStatus = status;
            }
            else
            {
                return BadRequest($"Invalid flight status: {request.FlightStatus}");
            }

            _context.Entry(flight).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                // Send SignalR notification to all clients about flight status update
                await _hubContext.Clients.All.SendAsync("FlightStatusUpdated", new
                {
                    FlightId = flight.FlightID,
                    FlightNumber = flight.FlightNumber,
                    Status = flight.FlightStatus.ToString(),
                    Gate = flight.Gate
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FlightExists(id))
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

        // DELETE: api/flights/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFlight(int id)
        {
            var flight = await _context.Flights.FindAsync(id);
            if (flight == null)
            {
                return NotFound();
            }

            _context.Flights.Remove(flight);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FlightExists(int id)
        {
            return _context.Flights.Any(e => e.FlightID == id);
        }
    }

    public class FlightStatusUpdateRequest
    {
        public string FlightStatus { get; set; } = string.Empty;
    }
}
