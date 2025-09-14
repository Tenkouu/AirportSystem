# API Documentation

## Overview

The Airport Management System API provides comprehensive endpoints for managing flights, passengers, seats, and check-in operations. All endpoints return JSON responses and support real-time updates via SignalR.

## Base URL
```
https://localhost:7xxx/api
```

## Authentication
Currently, the API does not require authentication. In production, implement proper authentication mechanisms.

## Response Format

### Success Response
```json
{
  "data": { ... },
  "status": "success"
}
```

### Error Response
```json
{
  "message": "Error description",
  "status": "error",
  "details": { ... }
}
```

## Endpoints

### Flights

#### Get All Flights
```http
GET /api/flights
```

**Response:**
```json
[
  {
    "flightID": 1,
    "flightNumber": "EK201",
    "arrivalAirport": "Dubai DXB",
    "destinationAirport": "New York JFK",
    "time": "2025-09-15T08:30:00",
    "gate": "C22",
    "flightStatus": 0,
    "seats": [...],
    "passengers": [...]
  }
]
```

#### Get Flight by ID
```http
GET /api/flights/{id}
```

**Parameters:**
- `id` (int): Flight ID

**Response:** Single flight object with seats and passengers

#### Create Flight
```http
POST /api/flights
Content-Type: application/json

{
  "flightNumber": "AA123",
  "arrivalAirport": "New York JFK",
  "destinationAirport": "Los Angeles LAX",
  "time": "2025-09-16T10:00:00",
  "gate": "A15",
  "flightStatus": 0
}
```

#### Update Flight
```http
PUT /api/flights/{id}
Content-Type: application/json

{
  "flightID": 1,
  "flightNumber": "EK201",
  "arrivalAirport": "Dubai DXB",
  "destinationAirport": "New York JFK",
  "time": "2025-09-15T08:30:00",
  "gate": "C22",
  "flightStatus": 1
}
```

#### Update Flight Status
```http
PUT /api/flights/{id}/status
Content-Type: application/json

{
  "flightStatus": "Boarding"
}
```

**Valid Status Values:**
- `CheckingIn`
- `Boarding`
- `Departed`
- `Delayed`
- `Cancelled`

**Real-time Update:** This endpoint triggers a SignalR broadcast to all connected clients.

#### Delete Flight
```http
DELETE /api/flights/{id}
```

### Passengers

#### Get All Passengers
```http
GET /api/passengers?passport={passportNumber}
```

**Query Parameters:**
- `passport` (string, optional): Filter by passport number

#### Get Passenger by ID
```http
GET /api/passengers/{id}
```

#### Create Passenger
```http
POST /api/passengers
Content-Type: application/json

{
  "fullName": "John Doe",
  "passportNumber": "P1234567",
  "flightID": 1,
  "assignedSeatID": null,
  "isCheckedIn": false
}
```

#### Update Passenger
```http
PUT /api/passengers/{id}
Content-Type: application/json

{
  "passengerID": 1,
  "fullName": "John Doe",
  "passportNumber": "P1234567",
  "flightID": 1,
  "assignedSeatID": 1,
  "isCheckedIn": true
}
```

#### Delete Passenger
```http
DELETE /api/passengers/{id}
```

### Seats

#### Get All Seats
```http
GET /api/seats
```

#### Get Seat by ID
```http
GET /api/seats/{id}
```

#### Get Seats by Flight
```http
GET /api/seats/flight/{flightId}
```

#### Create Seat
```http
POST /api/seats
Content-Type: application/json

{
  "flightID": 1,
  "seatNumber": "1A",
  "isOccupied": false,
  "passengerID": null
}
```

#### Update Seat
```http
PUT /api/seats/{id}
Content-Type: application/json

{
  "seatID": 1,
  "flightID": 1,
  "seatNumber": "1A",
  "isOccupied": true,
  "passengerID": 1
}
```

#### Occupy Seat
```http
POST /api/seats/occupy
Content-Type: application/json

{
  "seatId": 1,
  "passengerId": 1
}
```

**Real-time Update:** This endpoint triggers a SignalR broadcast to all clients in the flight group.

#### Release Seat
```http
POST /api/seats/release
Content-Type: application/json

{
  "seatId": 1
}
```

**Real-time Update:** This endpoint triggers a SignalR broadcast to all clients in the flight group.

#### Delete Seat
```http
DELETE /api/seats/{id}
```

### Check-in

#### Check-in Passenger
```http
POST /api/checkin
Content-Type: application/json

{
  "passportNumber": "LSM890123",
  "selectedSeatNumber": "1A"
}
```

**Request Body:**
- `passportNumber` (string, required): Passenger's passport number
- `selectedSeatNumber` (string, optional): Preferred seat number

**Response:**
```json
{
  "success": true,
  "message": "Check-in successful",
  "passengerName": "Liam Smith",
  "flightNumber": "EK201",
  "seatNumber": "1A",
  "gate": "C22"
}
```

**Real-time Update:** This endpoint triggers a SignalR broadcast when a seat is assigned.

## Error Codes

| Status Code | Description |
|-------------|-------------|
| 200 | Success |
| 201 | Created |
| 204 | No Content |
| 400 | Bad Request |
| 404 | Not Found |
| 500 | Internal Server Error |

## Rate Limiting

Currently, no rate limiting is implemented. Consider implementing rate limiting for production use.

## CORS

The API supports CORS for the following origins:
- `http://0.0.0.0:5001`
- `https://0.0.0.0:5001`

## SignalR Events

### Flight Status Updates
```javascript
connection.on("FlightStatusUpdated", function (update) {
    console.log("Flight Status Updated:", update);
    // update.flightId, update.flightNumber, update.status, update.gate
});
```

### Seat Updates
```javascript
connection.on("SeatOccupied", function (seatNumber) {
    console.log("Seat Occupied:", seatNumber);
});

connection.on("SeatAvailable", function (seatNumber) {
    console.log("Seat Available:", seatNumber);
});

connection.on("SeatSelected", function (seatNumber) {
    console.log("Seat Selected:", seatNumber);
});

connection.on("SeatDeselected", function (seatNumber) {
    console.log("Seat Deselected:", seatNumber);
});
```

## Examples

### Complete Check-in Flow

1. **Get available seats for a flight:**
   ```http
   GET /api/seats/flight/1
   ```

2. **Check-in passenger with seat selection:**
   ```http
   POST /api/checkin
   Content-Type: application/json
   
   {
     "passportNumber": "LSM890123",
     "selectedSeatNumber": "1A"
   }
   ```

3. **Update flight status:**
   ```http
   PUT /api/flights/1/status
   Content-Type: application/json
   
   {
     "flightStatus": "Boarding"
   }
   ```

### Real-time Updates

Connect to SignalR hubs for real-time updates:

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:7xxx/flightHub")
    .build();

await connection.start();

// Join a flight group
await connection.invoke("JoinFlightGroup", 1);

// Listen for updates
connection.on("FlightStatusUpdated", function (update) {
    // Handle flight status update
});
```
