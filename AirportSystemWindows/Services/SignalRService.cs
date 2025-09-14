using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace AirportSystemWindows.Services
{
    public class SignalRService
    {
        private HubConnection? _connection;
        private readonly string _hubUrl;

        public event Action<string>? SeatOccupied;
        public event Action<string>? SeatAvailable;
        public event Action<FlightStatusUpdate>? FlightStatusUpdated;
        public event Action<string>? SeatSelected;
        public event Action<string>? SeatDeselected;

        public SignalRService()
        {
            // --- THIS IS THE CRITICAL FIX ---
            // It was likely still "localhost", now it points to your IP address.
            _hubUrl = "http://10.2.202.57:5000/seatHub";
        }

        public async Task ConnectAsync()
        {
            if (_connection != null && _connection.State != HubConnectionState.Disconnected)
                return;

            _connection = new HubConnectionBuilder()
                .WithUrl(_hubUrl)
                .WithAutomaticReconnect()
                .Build();

            _connection.On<string>("SeatOccupied", (seatNumber) => SeatOccupied?.Invoke(seatNumber));
            _connection.On<string>("SeatAvailable", (seatNumber) => SeatAvailable?.Invoke(seatNumber));
            _connection.On<string>("SeatSelected", (seatNumber) => SeatSelected?.Invoke(seatNumber));
            _connection.On<string>("SeatDeselected", (seatNumber) => SeatDeselected?.Invoke(seatNumber));

            // FlightStatusUpdated is handled by the other page's service, but we leave it
            // here in case it's ever needed, to avoid confusion.
            _connection.On<object>("FlightStatusUpdated", (flightUpdateObject) =>
            {
                var json = System.Text.Json.JsonSerializer.Serialize(flightUpdateObject);
                var flightUpdate = System.Text.Json.JsonSerializer.Deserialize<FlightStatusUpdate>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (flightUpdate != null)
                {
                    FlightStatusUpdated?.Invoke(flightUpdate);
                }
            });

            try
            {
                await _connection.StartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to connect to SignalR hub: {ex.Message}");
            }
        }

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