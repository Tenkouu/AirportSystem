using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace AP2.Services
{
    public class SignalRService
    {
        private HubConnection? _connection;
        private readonly string _hubUrl;

        public event Action<string>? SeatOccupied;
        public event Action<string>? SeatAvailable;
        public event Action<FlightStatusUpdate>? FlightStatusUpdated;

        public SignalRService()
        {
            _hubUrl = "http://localhost:5000/seatHub";
        }

        public async Task ConnectAsync()
        {
            if (_connection != null)
                return;

            _connection = new HubConnectionBuilder()
                .WithUrl(_hubUrl)
                .Build();

            // Register event handlers
            _connection.On<string>("SeatOccupied", (seatNumber) =>
            {
                SeatOccupied?.Invoke(seatNumber);
            });

            _connection.On<string>("SeatAvailable", (seatNumber) =>
            {
                SeatAvailable?.Invoke(seatNumber);
            });

            _connection.On<FlightStatusUpdate>("FlightStatusUpdated", (flightStatus) =>
            {
                FlightStatusUpdated?.Invoke(flightStatus);
            });

            try
            {
                await _connection.StartAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to connect to SignalR hub: {ex.Message}");
            }
        }

        public async Task JoinFlightGroupAsync(int flightId)
        {
            if (_connection?.State == HubConnectionState.Connected)
            {
                await _connection.InvokeAsync("JoinFlightGroup", flightId);
            }
        }

        public async Task LeaveFlightGroupAsync(int flightId)
        {
            if (_connection?.State == HubConnectionState.Connected)
            {
                await _connection.InvokeAsync("LeaveFlightGroup", flightId);
            }
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