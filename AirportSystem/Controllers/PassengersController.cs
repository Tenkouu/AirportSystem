using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AirportSystem.Data;
using AirportSystem.Models;

namespace AirportSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PassengersController : ControllerBase
    {
        private readonly AirportDbContext _context;

        public PassengersController(AirportDbContext context)
        {
            _context = context;
        }

        // GET: api/passengers?passport=ABC123
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

        // GET: api/passengers/5
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

        // POST: api/passengers
        [HttpPost]
        public async Task<ActionResult<Passenger>> PostPassenger(Passenger passenger)
        {
            _context.Passengers.Add(passenger);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPassenger", new { id = passenger.PassengerID }, passenger);
        }

        // PUT: api/passengers/5
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

        // DELETE: api/passengers/5
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

        private bool PassengerExists(int id)
        {
            return _context.Passengers.Any(e => e.PassengerID == id);
        }
    }
}
