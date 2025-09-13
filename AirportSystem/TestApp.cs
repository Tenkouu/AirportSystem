using Microsoft.EntityFrameworkCore;
using AirportSystem.Data;
using AirportSystem.Models;

namespace AirportSystem
{
    public class TestApp
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Testing Airport System...");
            
            // Test database connection
            var options = new DbContextOptionsBuilder<AirportDbContext>()
                .UseSqlite("Data Source=test_airport.db")
                .Options;

            using var context = new AirportDbContext(options);
            
            try
            {
                // Ensure database is created
                await context.Database.EnsureCreatedAsync();
                Console.WriteLine("✅ Database created successfully");

                // Test querying flights
                var flights = await context.Flights.ToListAsync();
                Console.WriteLine($"✅ Found {flights.Count} flights in database");

                // Test querying passengers
                var passengers = await context.Passengers.ToListAsync();
                Console.WriteLine($"✅ Found {passengers.Count} passengers in database");

                // Test querying seats
                var seats = await context.Seats.ToListAsync();
                Console.WriteLine($"✅ Found {seats.Count} seats in database");

                Console.WriteLine("✅ All tests passed! The application should work correctly.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
            }
        }
    }
}
