using System.Collections.Generic;
using System.Text.Json.Serialization;
using AirportSystemWindows.Services;

[JsonSourceGenerationOptions(WriteIndented = true, PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(List<FlightApiResponse>))]
[JsonSerializable(typeof(List<PassengerApiResponse>))]
[JsonSerializable(typeof(List<SeatApiResponse>))]
[JsonSerializable(typeof(CheckInApiResponse))]
[JsonSerializable(typeof(ErrorResponse))]
[JsonSerializable(typeof(CheckInRequest))] // <-- This line makes the fix in the other file work
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}