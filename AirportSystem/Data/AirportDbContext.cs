using Microsoft.EntityFrameworkCore;
using AirportSystem.Models;

namespace AirportSystem.Data
{
    public class AirportDbContext : DbContext
    {
        public AirportDbContext(DbContextOptions<AirportDbContext> options) : base(options)
        {
        }

        public DbSet<Flight> Flights { get; set; }
        public DbSet<Passenger> Passengers { get; set; }
        public DbSet<Seat> Seats { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Flight entity
            modelBuilder.Entity<Flight>(entity =>
            {
                entity.HasKey(e => e.FlightID);
                entity.Property(e => e.FlightNumber).IsRequired().HasMaxLength(10);
                entity.Property(e => e.ArrivalAirport).IsRequired().HasMaxLength(100);
                entity.Property(e => e.DestinationAirport).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Gate).IsRequired().HasMaxLength(10);
                entity.Property(e => e.FlightStatus).IsRequired();
            });

            // Configure Passenger entity
            modelBuilder.Entity<Passenger>(entity =>
            {
                entity.HasKey(e => e.PassengerID);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PassportNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.IsCheckedIn).IsRequired();

                // Configure unique constraint on PassportNumber
                entity.HasIndex(e => e.PassportNumber).IsUnique();

                // Configure foreign key relationships
                entity.HasOne(e => e.Flight)
                      .WithMany(f => f.Passengers)
                      .HasForeignKey(e => e.FlightID)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.AssignedSeat)
                      .WithOne(s => s.Passenger)
                      .HasForeignKey<Passenger>(e => e.AssignedSeatID)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Seat entity
            modelBuilder.Entity<Seat>(entity =>
            {
                entity.HasKey(e => e.SeatID);
                entity.Property(e => e.SeatNumber).IsRequired().HasMaxLength(10);
                entity.Property(e => e.IsOccupied).IsRequired();

                // Configure foreign key relationship
                entity.HasOne(e => e.Flight)
                      .WithMany(f => f.Seats)
                      .HasForeignKey(e => e.FlightID)
                      .OnDelete(DeleteBehavior.Cascade);

                // Configure one-to-one relationship with Passenger
                entity.HasOne(e => e.Passenger)
                      .WithOne(p => p.AssignedSeat)
                      .HasForeignKey<Passenger>(p => p.AssignedSeatID)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Flights
            modelBuilder.Entity<Flight>().HasData(
                new Flight
                {
                    FlightID = 1,
                    FlightNumber = "AA100",
                    ArrivalAirport = "New York JFK",
                    DestinationAirport = "Los Angeles LAX",
                    Time = DateTime.Now.AddHours(2),
                    Gate = "A12",
                    FlightStatus = FlightStatus.CheckingIn
                },
                new Flight
                {
                    FlightID = 2,
                    FlightNumber = "UA200",
                    ArrivalAirport = "Chicago O'Hare",
                    DestinationAirport = "Miami MIA",
                    Time = DateTime.Now.AddHours(4),
                    Gate = "B8",
                    FlightStatus = FlightStatus.Boarding
                },
                new Flight
                {
                    FlightID = 3,
                    FlightNumber = "DL300",
                    ArrivalAirport = "Atlanta ATL",
                    DestinationAirport = "Seattle SEA",
                    Time = DateTime.Now.AddHours(6),
                    Gate = "C15",
                    FlightStatus = FlightStatus.Delayed
                },
                new Flight
                {
                    FlightID = 4,
                    FlightNumber = "SW400",
                    ArrivalAirport = "Dallas DFW",
                    DestinationAirport = "Denver DEN",
                    Time = DateTime.Now.AddHours(8),
                    Gate = "D22",
                    FlightStatus = FlightStatus.CheckingIn
                },
                new Flight
                {
                    FlightID = 5,
                    FlightNumber = "BA500",
                    ArrivalAirport = "London LHR",
                    DestinationAirport = "New York JFK",
                    Time = DateTime.Now.AddHours(10),
                    Gate = "E5",
                    FlightStatus = FlightStatus.Boarding
                },
                new Flight
                {
                    FlightID = 6,
                    FlightNumber = "LH600",
                    ArrivalAirport = "Frankfurt FRA",
                    DestinationAirport = "Chicago O'Hare",
                    Time = DateTime.Now.AddHours(12),
                    Gate = "F12",
                    FlightStatus = FlightStatus.Departed
                },
                new Flight
                {
                    FlightID = 7,
                    FlightNumber = "AF700",
                    ArrivalAirport = "Paris CDG",
                    DestinationAirport = "Los Angeles LAX",
                    Time = DateTime.Now.AddHours(14),
                    Gate = "G8",
                    FlightStatus = FlightStatus.Cancelled
                },
                new Flight
                {
                    FlightID = 8,
                    FlightNumber = "JL800",
                    ArrivalAirport = "Tokyo NRT",
                    DestinationAirport = "San Francisco SFO",
                    Time = DateTime.Now.AddHours(16),
                    Gate = "H3",
                    FlightStatus = FlightStatus.CheckingIn
                },
                new Flight
                {
                    FlightID = 9,
                    FlightNumber = "KE900",
                    ArrivalAirport = "Seoul ICN",
                    DestinationAirport = "New York JFK",
                    Time = DateTime.Now.AddHours(18),
                    Gate = "I7",
                    FlightStatus = FlightStatus.Boarding
                },
                new Flight
                {
                    FlightID = 10,
                    FlightNumber = "SQ1000",
                    ArrivalAirport = "Singapore SIN",
                    DestinationAirport = "Los Angeles LAX",
                    Time = DateTime.Now.AddHours(20),
                    Gate = "J11",
                    FlightStatus = FlightStatus.Delayed
                }
            );

            // Seed Seats for Flight 1
            modelBuilder.Entity<Seat>().HasData(
                new Seat { SeatID = 1, FlightID = 1, SeatNumber = "1A", IsOccupied = true }, // John Doe
                new Seat { SeatID = 2, FlightID = 1, SeatNumber = "1B", IsOccupied = true }, // Jane Smith
                new Seat { SeatID = 3, FlightID = 1, SeatNumber = "2A", IsOccupied = false },
                new Seat { SeatID = 4, FlightID = 1, SeatNumber = "2B", IsOccupied = false },
                new Seat { SeatID = 5, FlightID = 1, SeatNumber = "3A", IsOccupied = false },
                new Seat { SeatID = 10, FlightID = 1, SeatNumber = "3B", IsOccupied = false },
                new Seat { SeatID = 11, FlightID = 1, SeatNumber = "4A", IsOccupied = false },
                new Seat { SeatID = 12, FlightID = 1, SeatNumber = "4B", IsOccupied = false },
                new Seat { SeatID = 13, FlightID = 1, SeatNumber = "5A", IsOccupied = false }
            );

            // Seed Seats for Flight 2
            modelBuilder.Entity<Seat>().HasData(
                new Seat { SeatID = 6, FlightID = 2, SeatNumber = "1A", IsOccupied = true }, // Bob Johnson
                new Seat { SeatID = 7, FlightID = 2, SeatNumber = "1B", IsOccupied = false },
                new Seat { SeatID = 8, FlightID = 2, SeatNumber = "2A", IsOccupied = false },
                new Seat { SeatID = 9, FlightID = 2, SeatNumber = "2B", IsOccupied = false },
                new Seat { SeatID = 14, FlightID = 2, SeatNumber = "3A", IsOccupied = false },
                new Seat { SeatID = 15, FlightID = 2, SeatNumber = "3B", IsOccupied = false },
                new Seat { SeatID = 16, FlightID = 2, SeatNumber = "4A", IsOccupied = false }
            );

            // Seed Seats for Flight 3 (DL300)
            modelBuilder.Entity<Seat>().HasData(
                new Seat { SeatID = 17, FlightID = 3, SeatNumber = "1A", IsOccupied = false },
                new Seat { SeatID = 18, FlightID = 3, SeatNumber = "1B", IsOccupied = false },
                new Seat { SeatID = 19, FlightID = 3, SeatNumber = "2A", IsOccupied = false },
                new Seat { SeatID = 20, FlightID = 3, SeatNumber = "2B", IsOccupied = false },
                new Seat { SeatID = 21, FlightID = 3, SeatNumber = "3A", IsOccupied = false }
            );

            // Seed Seats for Flight 4 (SW400)
            modelBuilder.Entity<Seat>().HasData(
                new Seat { SeatID = 22, FlightID = 4, SeatNumber = "1A", IsOccupied = false },
                new Seat { SeatID = 23, FlightID = 4, SeatNumber = "1B", IsOccupied = false },
                new Seat { SeatID = 24, FlightID = 4, SeatNumber = "2A", IsOccupied = false },
                new Seat { SeatID = 25, FlightID = 4, SeatNumber = "2B", IsOccupied = false }
            );

            // Seed Seats for Flight 5 (BA500)
            modelBuilder.Entity<Seat>().HasData(
                new Seat { SeatID = 26, FlightID = 5, SeatNumber = "1A", IsOccupied = false },
                new Seat { SeatID = 27, FlightID = 5, SeatNumber = "1B", IsOccupied = false },
                new Seat { SeatID = 28, FlightID = 5, SeatNumber = "2A", IsOccupied = false },
                new Seat { SeatID = 29, FlightID = 5, SeatNumber = "2B", IsOccupied = false }
            );

            // Seed Seats for Flight 6 (LH600)
            modelBuilder.Entity<Seat>().HasData(
                new Seat { SeatID = 30, FlightID = 6, SeatNumber = "1A", IsOccupied = false },
                new Seat { SeatID = 31, FlightID = 6, SeatNumber = "1B", IsOccupied = false },
                new Seat { SeatID = 32, FlightID = 6, SeatNumber = "2A", IsOccupied = false }
            );

            // Seed Seats for Flight 7 (AF700)
            modelBuilder.Entity<Seat>().HasData(
                new Seat { SeatID = 33, FlightID = 7, SeatNumber = "1A", IsOccupied = false },
                new Seat { SeatID = 34, FlightID = 7, SeatNumber = "1B", IsOccupied = false },
                new Seat { SeatID = 35, FlightID = 7, SeatNumber = "2A", IsOccupied = false }
            );

            // Seed Seats for Flight 8 (JL800)
            modelBuilder.Entity<Seat>().HasData(
                new Seat { SeatID = 36, FlightID = 8, SeatNumber = "1A", IsOccupied = false },
                new Seat { SeatID = 37, FlightID = 8, SeatNumber = "1B", IsOccupied = false },
                new Seat { SeatID = 38, FlightID = 8, SeatNumber = "2A", IsOccupied = false }
            );

            // Seed Seats for Flight 9 (KE900)
            modelBuilder.Entity<Seat>().HasData(
                new Seat { SeatID = 39, FlightID = 9, SeatNumber = "1A", IsOccupied = false },
                new Seat { SeatID = 40, FlightID = 9, SeatNumber = "1B", IsOccupied = false },
                new Seat { SeatID = 41, FlightID = 9, SeatNumber = "2A", IsOccupied = false }
            );

            // Seed Seats for Flight 10 (SQ1000)
            modelBuilder.Entity<Seat>().HasData(
                new Seat { SeatID = 42, FlightID = 10, SeatNumber = "1A", IsOccupied = false },
                new Seat { SeatID = 43, FlightID = 10, SeatNumber = "1B", IsOccupied = false },
                new Seat { SeatID = 44, FlightID = 10, SeatNumber = "2A", IsOccupied = false }
            );

            // Seed Passengers
            modelBuilder.Entity<Passenger>().HasData(
                new Passenger
                {
                    PassengerID = 1,
                    FullName = "John Doe",
                    PassportNumber = "P1234567",
                    FlightID = 1,
                    AssignedSeatID = 1,
                    IsCheckedIn = true
                },
                new Passenger
                {
                    PassengerID = 2,
                    FullName = "Jane Smith",
                    PassportNumber = "P2345678",
                    FlightID = 1,
                    AssignedSeatID = 2,
                    IsCheckedIn = true
                },
                new Passenger
                {
                    PassengerID = 3,
                    FullName = "Bob Johnson",
                    PassportNumber = "P3456789",
                    FlightID = 2,
                    IsCheckedIn = false
                },
                // Additional passengers who haven't checked in
                new Passenger
                {
                    PassengerID = 4,
                    FullName = "Alice Brown",
                    PassportNumber = "P4567890",
                    FlightID = 1,
                    IsCheckedIn = false
                },
                new Passenger
                {
                    PassengerID = 5,
                    FullName = "Charlie Wilson",
                    PassportNumber = "P5678901",
                    FlightID = 1,
                    IsCheckedIn = false
                },
                new Passenger
                {
                    PassengerID = 6,
                    FullName = "Diana Davis",
                    PassportNumber = "P6789012",
                    FlightID = 2,
                    IsCheckedIn = false
                },
                new Passenger
                {
                    PassengerID = 7,
                    FullName = "Eve Miller",
                    PassportNumber = "P7890123",
                    FlightID = 2,
                    IsCheckedIn = false
                },
                new Passenger
                {
                    PassengerID = 8,
                    FullName = "Frank Garcia",
                    PassportNumber = "P8901234",
                    FlightID = 1,
                    IsCheckedIn = false
                }
            );
        }
    }
}
