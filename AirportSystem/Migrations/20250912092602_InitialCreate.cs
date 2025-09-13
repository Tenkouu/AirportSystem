using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AirportSystem.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Flights",
                columns: table => new
                {
                    FlightID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FlightNumber = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    ArrivalAirport = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DestinationAirport = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Time = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Gate = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    FlightStatus = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flights", x => x.FlightID);
                });

            migrationBuilder.CreateTable(
                name: "Seats",
                columns: table => new
                {
                    SeatID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FlightID = table.Column<int>(type: "INTEGER", nullable: false),
                    SeatNumber = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    IsOccupied = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seats", x => x.SeatID);
                    table.ForeignKey(
                        name: "FK_Seats_Flights_FlightID",
                        column: x => x.FlightID,
                        principalTable: "Flights",
                        principalColumn: "FlightID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Passengers",
                columns: table => new
                {
                    PassengerID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PassportNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    FlightID = table.Column<int>(type: "INTEGER", nullable: false),
                    AssignedSeatID = table.Column<int>(type: "INTEGER", nullable: true),
                    IsCheckedIn = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Passengers", x => x.PassengerID);
                    table.ForeignKey(
                        name: "FK_Passengers_Flights_FlightID",
                        column: x => x.FlightID,
                        principalTable: "Flights",
                        principalColumn: "FlightID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Passengers_Seats_AssignedSeatID",
                        column: x => x.AssignedSeatID,
                        principalTable: "Seats",
                        principalColumn: "SeatID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "Flights",
                columns: new[] { "FlightID", "ArrivalAirport", "DestinationAirport", "FlightNumber", "FlightStatus", "Gate", "Time" },
                values: new object[,]
                {
                    { 1, "New York JFK", "Los Angeles LAX", "AA100", 0, "A12", new DateTime(2025, 9, 12, 19, 26, 1, 820, DateTimeKind.Local).AddTicks(68) },
                    { 2, "Chicago O'Hare", "Miami MIA", "UA200", 1, "B8", new DateTime(2025, 9, 12, 21, 26, 1, 820, DateTimeKind.Local).AddTicks(105) },
                    { 3, "Atlanta ATL", "Seattle SEA", "DL300", 3, "C15", new DateTime(2025, 9, 12, 23, 26, 1, 820, DateTimeKind.Local).AddTicks(108) },
                    { 4, "Dallas DFW", "Denver DEN", "SW400", 0, "D22", new DateTime(2025, 9, 13, 1, 26, 1, 820, DateTimeKind.Local).AddTicks(112) },
                    { 5, "London LHR", "New York JFK", "BA500", 1, "E5", new DateTime(2025, 9, 13, 3, 26, 1, 820, DateTimeKind.Local).AddTicks(115) },
                    { 6, "Frankfurt FRA", "Chicago O'Hare", "LH600", 2, "F12", new DateTime(2025, 9, 13, 5, 26, 1, 820, DateTimeKind.Local).AddTicks(118) },
                    { 7, "Paris CDG", "Los Angeles LAX", "AF700", 4, "G8", new DateTime(2025, 9, 13, 7, 26, 1, 820, DateTimeKind.Local).AddTicks(121) },
                    { 8, "Tokyo NRT", "San Francisco SFO", "JL800", 0, "H3", new DateTime(2025, 9, 13, 9, 26, 1, 820, DateTimeKind.Local).AddTicks(124) },
                    { 9, "Seoul ICN", "New York JFK", "KE900", 1, "I7", new DateTime(2025, 9, 13, 11, 26, 1, 820, DateTimeKind.Local).AddTicks(127) },
                    { 10, "Singapore SIN", "Los Angeles LAX", "SQ1000", 3, "J11", new DateTime(2025, 9, 13, 13, 26, 1, 820, DateTimeKind.Local).AddTicks(130) }
                });

            migrationBuilder.InsertData(
                table: "Passengers",
                columns: new[] { "PassengerID", "AssignedSeatID", "FlightID", "FullName", "IsCheckedIn", "PassportNumber" },
                values: new object[,]
                {
                    { 3, null, 2, "Bob Johnson", false, "P3456789" },
                    { 4, null, 1, "Alice Brown", false, "P4567890" },
                    { 5, null, 1, "Charlie Wilson", false, "P5678901" },
                    { 6, null, 2, "Diana Davis", false, "P6789012" },
                    { 7, null, 2, "Eve Miller", false, "P7890123" },
                    { 8, null, 1, "Frank Garcia", false, "P8901234" }
                });

            migrationBuilder.InsertData(
                table: "Seats",
                columns: new[] { "SeatID", "FlightID", "IsOccupied", "SeatNumber" },
                values: new object[,]
                {
                    { 1, 1, true, "1A" },
                    { 2, 1, true, "1B" },
                    { 3, 1, false, "2A" },
                    { 4, 1, false, "2B" },
                    { 5, 1, false, "3A" },
                    { 6, 2, true, "1A" },
                    { 7, 2, false, "1B" },
                    { 8, 2, false, "2A" },
                    { 9, 2, false, "2B" },
                    { 10, 1, false, "3B" },
                    { 11, 1, false, "4A" },
                    { 12, 1, false, "4B" },
                    { 13, 1, false, "5A" },
                    { 14, 2, false, "3A" },
                    { 15, 2, false, "3B" },
                    { 16, 2, false, "4A" },
                    { 17, 3, false, "1A" },
                    { 18, 3, false, "1B" },
                    { 19, 3, false, "2A" },
                    { 20, 3, false, "2B" },
                    { 21, 3, false, "3A" },
                    { 22, 4, false, "1A" },
                    { 23, 4, false, "1B" },
                    { 24, 4, false, "2A" },
                    { 25, 4, false, "2B" },
                    { 26, 5, false, "1A" },
                    { 27, 5, false, "1B" },
                    { 28, 5, false, "2A" },
                    { 29, 5, false, "2B" },
                    { 30, 6, false, "1A" },
                    { 31, 6, false, "1B" },
                    { 32, 6, false, "2A" },
                    { 33, 7, false, "1A" },
                    { 34, 7, false, "1B" },
                    { 35, 7, false, "2A" },
                    { 36, 8, false, "1A" },
                    { 37, 8, false, "1B" },
                    { 38, 8, false, "2A" },
                    { 39, 9, false, "1A" },
                    { 40, 9, false, "1B" },
                    { 41, 9, false, "2A" },
                    { 42, 10, false, "1A" },
                    { 43, 10, false, "1B" },
                    { 44, 10, false, "2A" }
                });

            migrationBuilder.InsertData(
                table: "Passengers",
                columns: new[] { "PassengerID", "AssignedSeatID", "FlightID", "FullName", "IsCheckedIn", "PassportNumber" },
                values: new object[,]
                {
                    { 1, 1, 1, "John Doe", true, "P1234567" },
                    { 2, 2, 1, "Jane Smith", true, "P2345678" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Passengers_AssignedSeatID",
                table: "Passengers",
                column: "AssignedSeatID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Passengers_FlightID",
                table: "Passengers",
                column: "FlightID");

            migrationBuilder.CreateIndex(
                name: "IX_Passengers_PassportNumber",
                table: "Passengers",
                column: "PassportNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Seats_FlightID",
                table: "Seats",
                column: "FlightID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Passengers");

            migrationBuilder.DropTable(
                name: "Seats");

            migrationBuilder.DropTable(
                name: "Flights");
        }
    }
}
