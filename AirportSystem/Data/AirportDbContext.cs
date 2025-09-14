using Microsoft.EntityFrameworkCore;
using AirportSystem.Models;
using System;
using System.Collections.Generic;

namespace AirportSystem.Data
{
    /// <summary>
    /// Entity Framework database context for the airport management system.
    /// Manages database operations for flights, passengers, and seats with proper relationships and constraints.
    /// </summary>
    public class AirportDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the AirportDbContext class.
        /// </summary>
        /// <param name="options">The database context options.</param>
        public AirportDbContext(DbContextOptions<AirportDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the flights database set.
        /// </summary>
        public DbSet<Flight> Flights { get; set; }

        /// <summary>
        /// Gets or sets the passengers database set.
        /// </summary>
        public DbSet<Passenger> Passengers { get; set; }

        /// <summary>
        /// Gets or sets the seats database set.
        /// </summary>
        public DbSet<Seat> Seats { get; set; }

        /// <summary>
        /// Configures the entity models and their relationships for the database.
        /// </summary>
        /// <param name="modelBuilder">The model builder used to configure entities.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Flight>(entity =>
            {
                entity.HasKey(e => e.FlightID);
                entity.Property(e => e.FlightNumber).IsRequired().HasMaxLength(10);
                entity.Property(e => e.ArrivalAirport).IsRequired().HasMaxLength(100);
                entity.Property(e => e.DestinationAirport).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Gate).IsRequired().HasMaxLength(10);
                entity.Property(e => e.FlightStatus).IsRequired();
            });

            modelBuilder.Entity<Passenger>(entity =>
            {
                entity.HasKey(e => e.PassengerID);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PassportNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.IsCheckedIn).IsRequired();

                entity.HasIndex(e => e.PassportNumber).IsUnique();

                entity.HasOne(e => e.Flight)
                      .WithMany(f => f.Passengers)
                      .HasForeignKey(e => e.FlightID)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.AssignedSeat)
                      .WithOne(s => s.Passenger)
                      .HasForeignKey<Passenger>(e => e.AssignedSeatID)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Seat>(entity =>
            {
                entity.HasKey(e => e.SeatID);
                entity.Property(e => e.SeatNumber).IsRequired().HasMaxLength(10);
                entity.Property(e => e.IsOccupied).IsRequired();

                entity.HasOne(e => e.Flight)
                      .WithMany(f => f.Seats)
                      .HasForeignKey(e => e.FlightID)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Passenger)
                      .WithOne(p => p.AssignedSeat)
                      .HasForeignKey<Passenger>(p => p.AssignedSeatID)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            SeedData(modelBuilder);
        }

        /// <summary>
        /// Seeds the database with initial sample data for flights, seats, and passengers.
        /// </summary>
        /// <param name="modelBuilder">The model builder used to configure seed data.</param>
        private void SeedData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Flight>().HasData(
                new Flight { FlightID = 1, FlightNumber = "EK201", ArrivalAirport = "Dubai DXB", DestinationAirport = "New York JFK", Time = DateTime.Parse("2025-09-15 08:30:00"), Gate = "C22", FlightStatus = FlightStatus.CheckingIn },
                new Flight { FlightID = 2, FlightNumber = "QF12", ArrivalAirport = "Sydney SYD", DestinationAirport = "Los Angeles LAX", Time = DateTime.Parse("2025-09-15 10:00:00"), Gate = "T1-45", FlightStatus = FlightStatus.CheckingIn },
                new Flight { FlightID = 3, FlightNumber = "BA289", ArrivalAirport = "London LHR", DestinationAirport = "San Francisco SFO", Time = DateTime.Parse("2025-09-15 11:15:00"), Gate = "5A", FlightStatus = FlightStatus.Boarding },
                new Flight { FlightID = 4, FlightNumber = "LH454", ArrivalAirport = "Frankfurt FRA", DestinationAirport = "Chicago ORD", Time = DateTime.Parse("2025-09-15 12:45:00"), Gate = "Z18", FlightStatus = FlightStatus.Delayed },
                new Flight { FlightID = 5, FlightNumber = "AC791", ArrivalAirport = "Toronto YYZ", DestinationAirport = "Tokyo NRT", Time = DateTime.Parse("2025-09-15 14:00:00"), Gate = "F81", FlightStatus = FlightStatus.CheckingIn },
                new Flight { FlightID = 6, FlightNumber = "SQ38", ArrivalAirport = "Singapore SIN", DestinationAirport = "Houston IAH", Time = DateTime.Parse("2025-09-15 15:20:00"), Gate = "B3", FlightStatus = FlightStatus.Boarding },
                new Flight { FlightID = 7, FlightNumber = "AF006", ArrivalAirport = "Paris CDG", DestinationAirport = "Miami MIA", Time = DateTime.Parse("2025-09-15 16:05:00"), Gate = "K32", FlightStatus = FlightStatus.Cancelled },
                new Flight { FlightID = 8, FlightNumber = "JL005", ArrivalAirport = "Tokyo HND", DestinationAirport = "London LHR", Time = DateTime.Parse("2025-09-15 17:55:00"), Gate = "112", FlightStatus = FlightStatus.CheckingIn },
                new Flight { FlightID = 9, FlightNumber = "KE017", ArrivalAirport = "Seoul ICN", DestinationAirport = "Vancouver YVR", Time = DateTime.Parse("2025-09-15 19:10:00"), Gate = "2A", FlightStatus = FlightStatus.Departed },
                new Flight { FlightID = 10, FlightNumber = "UA870", ArrivalAirport = "Sydney SYD", DestinationAirport = "San Francisco SFO", Time = DateTime.Parse("2025-09-15 21:00:00"), Gate = "G91", FlightStatus = FlightStatus.Delayed }
            );

            var seats = new List<Seat>();
            int seatIdCounter = 1;
            var seatNumbers = new[] { "1A", "1B", "2A", "2B", "3A", "3B", "4A", "4B", "5A", "5B" };

            for (int flightId = 1; flightId <= 10; flightId++)
            {
                foreach (var seatNum in seatNumbers)
                {
                    seats.Add(new Seat
                    {
                        SeatID = seatIdCounter++,
                        FlightID = flightId,
                        SeatNumber = seatNum,
                        IsOccupied = false
                    });
                }
            }
            modelBuilder.Entity<Seat>().HasData(seats);

            modelBuilder.Entity<Passenger>().HasData(
                new Passenger { PassengerID = 1, FullName = "Liam Smith", PassportNumber = "LSM890123", FlightID = 1, AssignedSeatID = null, IsCheckedIn = false },
                new Passenger { PassengerID = 2, FullName = "Olivia Johnson", PassportNumber = "OJO123456", FlightID = 1, AssignedSeatID = null, IsCheckedIn = false },
                new Passenger { PassengerID = 3, FullName = "Noah Williams", PassportNumber = "NWI456789", FlightID = 1, AssignedSeatID = null, IsCheckedIn = false },
                new Passenger { PassengerID = 4, FullName = "Sophia Chen", PassportNumber = "SCH555111", FlightID = 1, AssignedSeatID = null, IsCheckedIn = false },
                new Passenger { PassengerID = 5, FullName = "Emma Brown", PassportNumber = "EBR789012", FlightID = 2, AssignedSeatID = null, IsCheckedIn = false },
                new Passenger { PassengerID = 6, FullName = "Oliver Jones", PassportNumber = "OJO345678", FlightID = 2, AssignedSeatID = null, IsCheckedIn = false },
                new Passenger { PassengerID = 7, FullName = "Chloe Green", PassportNumber = "CGR222333", FlightID = 2, AssignedSeatID = null, IsCheckedIn = false },
                new Passenger { PassengerID = 8, FullName = "Leo Taylor", PassportNumber = "LTA444555", FlightID = 2, AssignedSeatID = null, IsCheckedIn = false },
                new Passenger { PassengerID = 9, FullName = "Ava Garcia", PassportNumber = "AGA901234", FlightID = 3, AssignedSeatID = null, IsCheckedIn = false },
                new Passenger { PassengerID = 10, FullName = "Elijah Miller", PassportNumber = "EMI567890", FlightID = 3, AssignedSeatID = null, IsCheckedIn = false },
                new Passenger { PassengerID = 11, FullName = "Charlotte Davis", PassportNumber = "CDA123789", FlightID = 3, AssignedSeatID = null, IsCheckedIn = false },
                new Passenger { PassengerID = 12, FullName = "James Rodriguez", PassportNumber = "JRO456123", FlightID = 3, AssignedSeatID = null, IsCheckedIn = false },
                new Passenger { PassengerID = 13, FullName = "Isla Walker", PassportNumber = "IWA666777", FlightID = 3, AssignedSeatID = null, IsCheckedIn = false },
                new Passenger { PassengerID = 14, FullName = "Sophia Wilson", PassportNumber = "SWI789456", FlightID = 4, AssignedSeatID = null, IsCheckedIn = false },
                new Passenger { PassengerID = 15, FullName = "William Martinez", PassportNumber = "WMA012345", FlightID = 4, AssignedSeatID = null, IsCheckedIn = false },
                new Passenger { PassengerID = 16, FullName = "Zoe Hall", PassportNumber = "ZHA888999", FlightID = 4, AssignedSeatID = null, IsCheckedIn = false },
                new Passenger { PassengerID = 17, FullName = "Jack Lewis", PassportNumber = "JLE111000", FlightID = 4, AssignedSeatID = null, IsCheckedIn = false },
                new Passenger { PassengerID = 18, FullName = "Isabella Anderson", PassportNumber = "IAN345012", FlightID = 5, AssignedSeatID = null, IsCheckedIn = false },
                new Passenger { PassengerID = 19, FullName = "Henry Taylor", PassportNumber = "HTA678345", FlightID = 5, AssignedSeatID = null, IsCheckedIn = false },
                new Passenger { PassengerID = 20, FullName = "Mia Thomas", PassportNumber = "MTH901678", FlightID = 5, AssignedSeatID = null, IsCheckedIn = false },
                new Passenger { PassengerID = 21, FullName = "Lucas Clark", PassportNumber = "LCL777888", FlightID = 5, AssignedSeatID = null, IsCheckedIn = false },
                new Passenger { PassengerID = 22, FullName = "Grace Scott", PassportNumber = "GSC333444", FlightID = 5, AssignedSeatID = null, IsCheckedIn = false },
                new Passenger { PassengerID = 23, FullName = "Theodore Hernandez", PassportNumber = "THE234901", FlightID = 6, AssignedSeatID = null, IsCheckedIn = false },
                new Passenger { PassengerID = 24, FullName = "Evelyn Moore", PassportNumber = "EMO567234", FlightID = 6, AssignedSeatID = null, IsCheckedIn = false },
                new Passenger { PassengerID = 25, FullName = "Aria Nelson", PassportNumber = "ANE999000", FlightID = 6, AssignedSeatID = null, IsCheckedIn = false },
                new Passenger { PassengerID = 26, FullName = "Lucas Martin", PassportNumber = "LMA890567", FlightID = 8, AssignedSeatID = null, IsCheckedIn = false },
                new Passenger { PassengerID = 27, FullName = "Harper Jackson", PassportNumber = "HJA123890", FlightID = 8, AssignedSeatID = null, IsCheckedIn = false },
                new Passenger { PassengerID = 28, FullName = "Mateo King", PassportNumber = "MKI555666", FlightID = 8, AssignedSeatID = null, IsCheckedIn = false },
                new Passenger { PassengerID = 29, FullName = "Aurora Wright", PassportNumber = "AWR222111", FlightID = 8, AssignedSeatID = null, IsCheckedIn = false },
                new Passenger { PassengerID = 30, FullName = "Benjamin Thompson", PassportNumber = "BTH456123", FlightID = 10, AssignedSeatID = null, IsCheckedIn = false },
                new Passenger { PassengerID = 31, FullName = "Amelia White", PassportNumber = "AWH789456", FlightID = 10, AssignedSeatID = null, IsCheckedIn = false },
                new Passenger { PassengerID = 32, FullName = "Logan Harris", PassportNumber = "LHA888777", FlightID = 10, AssignedSeatID = null, IsCheckedIn = false },
                new Passenger { PassengerID = 33, FullName = "Penelope Allen", PassportNumber = "PAL444333", FlightID = 10, AssignedSeatID = null, IsCheckedIn = false }
            );
        }
    }
}