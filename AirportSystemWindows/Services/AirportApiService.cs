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
        private readonly JsonSerializerOptions _serializerOptions;

        public AirportApiService()
        {
            _httpClient = new HttpClient();
            _baseUrl = "http://localhost:5000/api"; 

            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<List<FlightApiResponse>> GetFlightsAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"{_baseUrl}/flights");
                var flights = JsonSerializer.Deserialize<List<FlightApiResponse>>(response, _serializerOptions);
                return flights ?? new List<FlightApiResponse>();
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
                var response = await _httpClient.GetStringAsync($"{_baseUrl}/passengers");
                var passengers = JsonSerializer.Deserialize<List<PassengerApiResponse>>(response, _serializerOptions);
                return passengers?.FirstOrDefault(p => p.PassportNumber == passportNumber);
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
                var request = new { passportNumber, selectedSeatNumber };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/checkin", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonSerializer.Deserialize<CheckInApiResponse>(responseContent, _serializerOptions);
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseContent, _serializerOptions);
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
                var seats = JsonSerializer.Deserialize<List<SeatApiResponse>>(response, _serializerOptions);
                return seats ?? new List<SeatApiResponse>();
            }
            catch (Exception ex) { throw new Exception($"Failed to fetch seats: {ex.Message}"); }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    // API Response DTOs
    public class FlightApiResponse
    {
        public FlightApiResponse() { } // Explicit parameterless constructor
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
        public PassengerApiResponse() { } // Explicit parameterless constructor
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
        public SeatApiResponse() { } // Explicit parameterless constructor
        public int SeatID { get; set; }
        public int FlightID { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public bool IsOccupied { get; set; }
        public FlightApiResponse? Flight { get; set; }
        public PassengerApiResponse? Passenger { get; set; }
    }

    public class CheckInApiResponse
    {
        public CheckInApiResponse() { } // Explicit parameterless constructor
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string PassengerName { get; set; } = string.Empty;
        public string FlightNumber { get; set; } = string.Empty;
        public string SeatNumber { get; set; } = string.Empty;
        public string Gate { get; set; } = string.Empty;
    }

    public class ErrorResponse
    {
        public ErrorResponse() { } // Explicit parameterless constructor
        public string Message { get; set; } = string.Empty;
    }
}