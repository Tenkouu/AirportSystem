using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace AirportSystemWindows.Services
{
    public class SignalRService
    {
        private HubConnection? _connection;
        private readonly string _hubUrl;

        // Events the UI can subscribe to
        public event Action<string>? SeatOccupied;
        public event Action<string>? SeatAvailable;
        public event Action<FlightStatusUpdate>? FlightStatusUpdated;
        public event Action<string>? SeatSelected;   // A seat was soft-locked by another user
        public event Action<string>? SeatDeselected; // A seat was soft-unlocked by another user

        public SignalRService()
        {
            _hubUrl = "http://localhost:5000/seatHub";
        }

        public async Task ConnectAsync()
        {
            if (_connection != null && _connection.State != HubConnectionState.Disconnected)
                return;

            _connection = new HubConnectionBuilder()
                .WithUrl(_hubUrl)
                .WithAutomaticReconnect() // Good practice for resilience
                .Build();

            // Register event handlers for messages from the server
            _connection.On<string>("SeatOccupied", (seatNumber) => SeatOccupied?.Invoke(seatNumber));
            _connection.On<string>("SeatAvailable", (seatNumber) => SeatAvailable?.Invoke(seatNumber));
            _connection.On<FlightStatusUpdate>("FlightStatusUpdated", (flightStatus) => FlightStatusUpdated?.Invoke(flightStatus));
            _connection.On<string>("SeatSelected", (seatNumber) => SeatSelected?.Invoke(seatNumber));
            _connection.On<string>("SeatDeselected", (seatNumber) => SeatDeselected?.Invoke(seatNumber));

            try
            {
                await _connection.StartAsync();
            }
            catch (Exception ex)
            {
                // Don't throw, let the UI handle the disconnected state
                Console.WriteLine($"Failed to connect to SignalR hub: {ex.Message}");
            }
        }

        // Methods to send messages to the server
        public async Task SelectSeatAsync(int flightId, string seatNumber)
        {
            if (IsConnected) await _connection.InvokeAsync("SelectSeat", flightId, seatNumber);
        }

        public async Task DeselectSeatAsync(int flightId, string seatNumber)
        {
            if (IsConnected) await _connection.InvokeAsync("DeselectSeat", flightId, seatNumber);
        }

        public async Task JoinFlightGroupAsync(int flightId)
        {
            if (IsConnected) await _connection.InvokeAsync("JoinFlightGroup", flightId);
        }

        public async Task LeaveFlightGroupAsync(int flightId)
        {
            if (IsConnected) await _connection.InvokeAsync("LeaveFlightGroup", flightId);
        }

        public async Task DisconnectAsync()
        {
            if (_connection != null)
            {
                await _connection.DisposeAsync();
                _connection = null;
            }
        }

        public bool IsConnected => _connection?.State == HubConnectionState.Connected;
    }

    public class FlightStatusUpdate
    {
        public int FlightId { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Gate { get; set; } = string.Empty;
    }
}