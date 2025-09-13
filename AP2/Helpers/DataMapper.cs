using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using AP2.Services;
using static AP2.MainWindow;

namespace AP2.Helpers
{
    public static class DataMapper
    {
        public static FlightInfo MapToFlightInfo(FlightApiResponse apiFlight)
        {
            return new FlightInfo
            {
                FlightNumber = apiFlight.FlightNumber,
                DepartureAirport = apiFlight.ArrivalAirport, // Note: API uses ArrivalAirport for departure
                ArrivalAirport = apiFlight.DestinationAirport,
                ScheduledTime = apiFlight.Time.ToString("HH:mm"),
                Status = MapFlightStatus(apiFlight.FlightStatus),
                Gate = apiFlight.Gate,
                StatusColor = GetStatusColor(apiFlight.FlightStatus)
            };
        }

        public static PassengerInfo MapToPassengerInfo(PassengerApiResponse apiPassenger)
        {
            return new PassengerInfo
            {
                Name = apiPassenger.FullName,
                Passport = apiPassenger.PassportNumber,
                Flight = apiPassenger.Flight != null ? 
                    $"{apiPassenger.Flight.FlightNumber} - {apiPassenger.Flight.ArrivalAirport} → {apiPassenger.Flight.DestinationAirport}" : 
                    "Unknown Flight",
                Seat = apiPassenger.AssignedSeat?.SeatNumber,
                Status = apiPassenger.IsCheckedIn ? "Checked In" : "Pending Check-in"
            };
        }

        public static Dictionary<string, bool> MapToSeatMap(List<SeatApiResponse> seats)
        {
            var seatMap = new Dictionary<string, bool>();
            foreach (var seat in seats)
            {
                seatMap[seat.SeatNumber] = seat.IsOccupied;
            }
            return seatMap;
        }

        private static string MapFlightStatus(int apiStatus)
        {
            return apiStatus switch
            {
                0 => "Checking In",    // CheckingIn
                1 => "Boarding",       // Boarding
                2 => "Departed",       // Departed
                3 => "Delayed",        // Delayed
                4 => "Cancelled",      // Cancelled
                _ => "Unknown"
            };
        }

        private static SolidColorBrush GetStatusColor(int apiStatus)
        {
            return apiStatus switch
            {
                0 => new SolidColorBrush(Colors.Green),    // CheckingIn
                1 => new SolidColorBrush(Colors.Blue),     // Boarding
                2 => new SolidColorBrush(Colors.Pink),     // Departed
                3 => new SolidColorBrush(Colors.Orange),   // Delayed
                4 => new SolidColorBrush(Colors.Red),      // Cancelled
                _ => new SolidColorBrush(Colors.Gray)
            };
        }

        public static string MapStatusToApi(string uiStatus)
        {
            return uiStatus switch
            {
                "Checking In" => "CheckingIn",
                "Boarding" => "Boarding",
                "Departed" => "Departed",
                "Delayed" => "Delayed",
                "Cancelled" => "Cancelled",
                _ => "CheckingIn"
            };
        }
    }
}
