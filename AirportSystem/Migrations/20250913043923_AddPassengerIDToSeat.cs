using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirportSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddPassengerIDToSeat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PassengerID",
                table: "Seats",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Flights",
                keyColumn: "FlightID",
                keyValue: 1,
                column: "Time",
                value: new DateTime(2025, 9, 13, 14, 39, 23, 318, DateTimeKind.Local).AddTicks(3448));

            migrationBuilder.UpdateData(
                table: "Flights",
                keyColumn: "FlightID",
                keyValue: 2,
                column: "Time",
                value: new DateTime(2025, 9, 13, 16, 39, 23, 318, DateTimeKind.Local).AddTicks(3470));

            migrationBuilder.UpdateData(
                table: "Flights",
                keyColumn: "FlightID",
                keyValue: 3,
                column: "Time",
                value: new DateTime(2025, 9, 13, 18, 39, 23, 318, DateTimeKind.Local).AddTicks(3472));

            migrationBuilder.UpdateData(
                table: "Flights",
                keyColumn: "FlightID",
                keyValue: 4,
                column: "Time",
                value: new DateTime(2025, 9, 13, 20, 39, 23, 318, DateTimeKind.Local).AddTicks(3474));

            migrationBuilder.UpdateData(
                table: "Flights",
                keyColumn: "FlightID",
                keyValue: 5,
                column: "Time",
                value: new DateTime(2025, 9, 13, 22, 39, 23, 318, DateTimeKind.Local).AddTicks(3476));

            migrationBuilder.UpdateData(
                table: "Flights",
                keyColumn: "FlightID",
                keyValue: 6,
                column: "Time",
                value: new DateTime(2025, 9, 14, 0, 39, 23, 318, DateTimeKind.Local).AddTicks(3478));

            migrationBuilder.UpdateData(
                table: "Flights",
                keyColumn: "FlightID",
                keyValue: 7,
                column: "Time",
                value: new DateTime(2025, 9, 14, 2, 39, 23, 318, DateTimeKind.Local).AddTicks(3479));

            migrationBuilder.UpdateData(
                table: "Flights",
                keyColumn: "FlightID",
                keyValue: 8,
                column: "Time",
                value: new DateTime(2025, 9, 14, 4, 39, 23, 318, DateTimeKind.Local).AddTicks(3481));

            migrationBuilder.UpdateData(
                table: "Flights",
                keyColumn: "FlightID",
                keyValue: 9,
                column: "Time",
                value: new DateTime(2025, 9, 14, 6, 39, 23, 318, DateTimeKind.Local).AddTicks(3483));

            migrationBuilder.UpdateData(
                table: "Flights",
                keyColumn: "FlightID",
                keyValue: 10,
                column: "Time",
                value: new DateTime(2025, 9, 14, 8, 39, 23, 318, DateTimeKind.Local).AddTicks(3484));

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 1,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 2,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 3,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 4,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 5,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 6,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 7,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 8,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 9,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 10,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 11,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 12,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 13,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 14,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 15,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 16,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 17,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 18,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 19,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 20,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 21,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 22,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 23,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 24,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 25,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 26,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 27,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 28,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 29,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 30,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 31,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 32,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 33,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 34,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 35,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 36,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 37,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 38,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 39,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 40,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 41,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 42,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 43,
                column: "PassengerID",
                value: null);

            migrationBuilder.UpdateData(
                table: "Seats",
                keyColumn: "SeatID",
                keyValue: 44,
                column: "PassengerID",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PassengerID",
                table: "Seats");

            migrationBuilder.UpdateData(
                table: "Flights",
                keyColumn: "FlightID",
                keyValue: 1,
                column: "Time",
                value: new DateTime(2025, 9, 12, 19, 26, 1, 820, DateTimeKind.Local).AddTicks(68));

            migrationBuilder.UpdateData(
                table: "Flights",
                keyColumn: "FlightID",
                keyValue: 2,
                column: "Time",
                value: new DateTime(2025, 9, 12, 21, 26, 1, 820, DateTimeKind.Local).AddTicks(105));

            migrationBuilder.UpdateData(
                table: "Flights",
                keyColumn: "FlightID",
                keyValue: 3,
                column: "Time",
                value: new DateTime(2025, 9, 12, 23, 26, 1, 820, DateTimeKind.Local).AddTicks(108));

            migrationBuilder.UpdateData(
                table: "Flights",
                keyColumn: "FlightID",
                keyValue: 4,
                column: "Time",
                value: new DateTime(2025, 9, 13, 1, 26, 1, 820, DateTimeKind.Local).AddTicks(112));

            migrationBuilder.UpdateData(
                table: "Flights",
                keyColumn: "FlightID",
                keyValue: 5,
                column: "Time",
                value: new DateTime(2025, 9, 13, 3, 26, 1, 820, DateTimeKind.Local).AddTicks(115));

            migrationBuilder.UpdateData(
                table: "Flights",
                keyColumn: "FlightID",
                keyValue: 6,
                column: "Time",
                value: new DateTime(2025, 9, 13, 5, 26, 1, 820, DateTimeKind.Local).AddTicks(118));

            migrationBuilder.UpdateData(
                table: "Flights",
                keyColumn: "FlightID",
                keyValue: 7,
                column: "Time",
                value: new DateTime(2025, 9, 13, 7, 26, 1, 820, DateTimeKind.Local).AddTicks(121));

            migrationBuilder.UpdateData(
                table: "Flights",
                keyColumn: "FlightID",
                keyValue: 8,
                column: "Time",
                value: new DateTime(2025, 9, 13, 9, 26, 1, 820, DateTimeKind.Local).AddTicks(124));

            migrationBuilder.UpdateData(
                table: "Flights",
                keyColumn: "FlightID",
                keyValue: 9,
                column: "Time",
                value: new DateTime(2025, 9, 13, 11, 26, 1, 820, DateTimeKind.Local).AddTicks(127));

            migrationBuilder.UpdateData(
                table: "Flights",
                keyColumn: "FlightID",
                keyValue: 10,
                column: "Time",
                value: new DateTime(2025, 9, 13, 13, 26, 1, 820, DateTimeKind.Local).AddTicks(130));
        }
    }
}
