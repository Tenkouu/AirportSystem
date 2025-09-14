using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AirportSystemWindows.Services
{
    public class AirportApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public AirportApiService()
        {
            _httpClient = new HttpClient();
            _baseUrl = "http://10.3.202.148:5000/api";
        }

        public async Task<List<FlightApiResponse>> GetFlightsAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"{_baseUrl}/flights");
                return JsonSerializer.Deserialize(response, AppJsonSerializerContext.Default.ListFlightApiResponse);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to fetch flights: {ex.Message}");
            }
        }

        public async Task<PassengerApiResponse> GetPassengerByPassportAsync(string passportNumber)
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"{_baseUrl}/passengers?passport={passportNumber}");
                var passengers = JsonSerializer.Deserialize(response, AppJsonSerializerContext.Default.ListPassengerApiResponse);
                return passengers?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to fetch passenger: {ex.Message}");
            }
        }

        public async Task<CheckInApiResponse> CheckInPassengerAsync(string passportNumber, string? selectedSeatNumber = null)
        {
            try
            {
                // This is the fix: Use a real, named class to avoid trimming issues.
                var request = new CheckInRequest
                {
                    PassportNumber = passportNumber,
                    SelectedSeatNumber = selectedSeatNumber
                };

                // Use the source generator to serialize the new request type.
                var json = JsonSerializer.Serialize(request, AppJsonSerializerContext.Default.CheckInRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/checkin", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonSerializer.Deserialize(responseContent, AppJsonSerializerContext.Default.CheckInApiResponse);
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize(responseContent, AppJsonSerializerContext.Default.ErrorResponse);
                    throw new Exception(errorResponse?.Message ?? "Check-in failed");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to check-in passenger: {ex.Message}");
            }
        }

        public async Task<bool> UpdateFlightStatusAsync(int flightId, string status)
        {
            try
            {
                var request = new { flightStatus = status };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_baseUrl}/flights/{flightId}/status", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex) { throw new Exception($"Failed to update flight status: {ex.Message}"); }
        }

        public async Task<List<SeatApiResponse>> GetSeatsByFlightAsync(int flightId)
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"{_baseUrl}/seats/flight/{flightId}");
                return JsonSerializer.Deserialize(response, AppJsonSerializerContext.Default.ListSeatApiResponse);
            }
            catch (Exception ex) { throw new Exception($"Failed to fetch seats: {ex.Message}"); }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    // --- Data Transfer Objects (DTOs) ---

    // The new, named request class to prevent trimming errors.
    public class CheckInRequest
    {
        public string PassportNumber { get; set; }
        public string? SelectedSeatNumber { get; set; }
    }

    public class FlightApiResponse
    {
        public FlightApiResponse() { }
        public int FlightID { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public string ArrivalAirport { get; set; } = string.Empty;
        public string DestinationAirport { get; set; } = string.Empty;
        public DateTime Time { get; set; }
        public string Gate { get; set; } = string.Empty;
        public int FlightStatus { get; set; }
        public List<SeatApiResponse> Seats { get; set; } = new List<SeatApiResponse>();
        public List<PassengerApiResponse> Passengers { get; set; } = new List<PassengerApiResponse>();
    }

    public class PassengerApiResponse
    {
        public PassengerApiResponse() { }
        public int PassengerID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PassportNumber { get; set; } = string.Empty;
        public int FlightID { get; set; }
        public int? AssignedSeatID { get; set; }
        public bool IsCheckedIn { get; set; }
        public FlightApiResponse? Flight { get; set; }
        public SeatApiResponse? AssignedSeat { get; set; }
    }

    public class SeatApiResponse
    {
        public SeatApiResponse() { }
        public int SeatID { get; set; }
        public int FlightID { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public bool IsOccupied { get; set; }
        public int? PassengerID { get; set; }
        public FlightApiResponse? Flight { get; set; }
        public PassengerApiResponse? Passenger { get; set; }
    }

    public class CheckInApiResponse
    {
        public CheckInApiResponse() { }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string PassengerName { get; set; } = string.Empty;
        public string FlightNumber { get; set; } = string.Empty;
        public string SeatNumber { get; set; } = string.Empty;
        public string Gate { get; set; } = string.Empty;
    }

    public class ErrorResponse
    {
        public ErrorResponse() { }
        public string Message { get; set; } = string.Empty;
    }
}