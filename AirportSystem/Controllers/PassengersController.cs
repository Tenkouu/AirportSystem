using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AirportSystem.Data;
using AirportSystem.Models;

namespace AirportSystem.Controllers
{
    /// <summary>
    /// API controller for managing passengers in the airport system.
    /// Provides CRUD operations for passenger data and seat assignments.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PassengersController : ControllerBase
    {
        private readonly AirportDbContext _context;

        /// <summary>
        /// Initializes a new instance of the PassengersController class.
        /// </summary>
        /// <param name="context">The database context for passenger operations.</param>
        public PassengersController(AirportDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves passengers with optional filtering by passport number.
        /// </summary>
        /// <param name="passport">Optional passport number to filter passengers.</param>
        /// <returns>A collection of passengers matching the criteria.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Passenger>>> GetPassengers([FromQuery] string? passport)
        {
            var query = _context.Passengers
                .Include(p => p.Flight)
                .Include(p => p.AssignedSeat)
                .AsQueryable();

            if (!string.IsNullOrEmpty(passport))
            {
                query = query.Where(p => p.PassportNumber == passport);
            }

            return await query.ToListAsync();
        }

        /// <summary>
        /// Retrieves a specific passenger by their ID.
        /// </summary>
        /// <param name="id">The unique identifier of the passenger.</param>
        /// <returns>The passenger with the specified ID, or NotFound if not found.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Passenger>> GetPassenger(int id)
        {
            var passenger = await _context.Passengers
                .Include(p => p.Flight)
                .Include(p => p.AssignedSeat)
                .FirstOrDefaultAsync(p => p.PassengerID == id);

            if (passenger == null)
            {
                return NotFound();
            }

            return passenger;
        }

        /// <summary>
        /// Creates a new passenger in the system.
        /// </summary>
        /// <param name="passenger">The passenger object to create.</param>
        /// <returns>The created passenger with its assigned ID.</returns>
        [HttpPost]
        public async Task<ActionResult<Passenger>> PostPassenger(Passenger passenger)
        {
            _context.Passengers.Add(passenger);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPassenger", new { id = passenger.PassengerID }, passenger);
        }

        /// <summary>
        /// Updates an existing passenger.
        /// </summary>
        /// <param name="id">The ID of the passenger to update.</param>
        /// <param name="passenger">The updated passenger data.</param>
        /// <returns>NoContent if successful, BadRequest if IDs don't match, NotFound if passenger doesn't exist.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPassenger(int id, Passenger passenger)
        {
            if (id != passenger.PassengerID)
            {
                return BadRequest();
            }

            _context.Entry(passenger).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PassengerExists(id))
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
        /// Deletes a passenger from the system.
        /// </summary>
        /// <param name="id">The ID of the passenger to delete.</param>
        /// <returns>NoContent if successful, NotFound if passenger doesn't exist.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePassenger(int id)
        {
            var passenger = await _context.Passengers.FindAsync(id);
            if (passenger == null)
            {
                return NotFound();
            }

            _context.Passengers.Remove(passenger);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Checks if a passenger with the specified ID exists in the database.
        /// </summary>
        /// <param name="id">The passenger ID to check.</param>
        /// <returns>True if the passenger exists, false otherwise.</returns>
        private bool PassengerExists(int id)
        {
            return _context.Passengers.Any(e => e.PassengerID == id);
        }
    }
}
