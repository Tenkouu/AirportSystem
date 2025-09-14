using System.Collections.Generic;
using System.Text.Json.Serialization;
using AirportSystemWindows.Services;

// This class tells the .NET Source Generator which types to prepare for trimming.
// By defining these attributes, the compiler will generate reflection-free code
// to serialize/deserialize these specific types, making it safe for the trimmer.

[JsonSourceGenerationOptions(WriteIndented = true, PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(List<FlightApiResponse>))]
[JsonSerializable(typeof(List<PassengerApiResponse>))]
[JsonSerializable(typeof(List<SeatApiResponse>))]
[JsonSerializable(typeof(CheckInApiResponse))]
[JsonSerializable(typeof(ErrorResponse))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}