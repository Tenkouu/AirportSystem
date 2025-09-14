using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using AirportSystem.Data;
using AirportSystem.Models;
using AirportSystem.Hubs;

namespace AirportSystem.Controllers
{
    /// <summary>
    /// API controller for managing flights in the airport system.
    /// Provides CRUD operations and real-time status updates via SignalR.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class FlightsController : ControllerBase
    {
        private readonly AirportDbContext _context;
        private readonly IHubContext<FlightHub> _hubContext;

        /// <summary>
        /// Initializes a new instance of the FlightsController class.
        /// </summary>
        /// <param name="context">The database context for flight operations.</param>
        /// <param name="hubContext">The SignalR hub context for real-time notifications.</param>
        public FlightsController(AirportDbContext context, IHubContext<FlightHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Retrieves all flights with their associated seats and passengers.
        /// </summary>
        /// <returns>A collection of all flights in the system.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Flight>>> GetFlights()
        {
            return await _context.Flights
                .Include(f => f.Seats)
                .Include(f => f.Passengers)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a specific flight by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the flight.</param>
        /// <returns>The flight with the specified ID, or NotFound if not found.</returns>
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

        /// <summary>
        /// Creates a new flight in the system.
        /// </summary>
        /// <param name="flight">The flight object to create.</param>
        /// <returns>The created flight with its assigned ID.</returns>
        [HttpPost]
        public async Task<ActionResult<Flight>> PostFlight(Flight flight)
        {
            _context.Flights.Add(flight);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetFlight", new { id = flight.FlightID }, flight);
        }

        /// <summary>
        /// Updates an existing flight.
        /// </summary>
        /// <param name="id">The ID of the flight to update.</param>
        /// <param name="flight">The updated flight data.</param>
        /// <returns>NoContent if successful, BadRequest if IDs don't match, NotFound if flight doesn't exist.</returns>
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

        /// <summary>
        /// Updates the status of a specific flight and notifies all connected clients.
        /// </summary>
        /// <param name="id">The ID of the flight to update.</param>
        /// <param name="request">The status update request containing the new status.</param>
        /// <returns>NoContent if successful, NotFound if flight doesn't exist, BadRequest if status is invalid.</returns>
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateFlightStatus(int id, [FromBody] FlightStatusUpdateRequest request)
        {
            var flight = await _context.Flights.FindAsync(id);
            if (flight == null)
            {
                return NotFound();
            }

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

        /// <summary>
        /// Deletes a flight from the system.
        /// </summary>
        /// <param name="id">The ID of the flight to delete.</param>
        /// <returns>NoContent if successful, NotFound if flight doesn't exist.</returns>
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

        /// <summary>
        /// Checks if a flight with the specified ID exists in the database.
        /// </summary>
        /// <param name="id">The flight ID to check.</param>
        /// <returns>True if the flight exists, false otherwise.</returns>
        private bool FlightExists(int id)
        {
            return _context.Flights.Any(e => e.FlightID == id);
        }
    }

    /// <summary>
    /// Request model for updating flight status.
    /// </summary>
    public class FlightStatusUpdateRequest
    {
        /// <summary>
        /// Gets or sets the new flight status as a string.
        /// </summary>
        public string FlightStatus { get; set; } = string.Empty;
    }
}
