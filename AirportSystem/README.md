# Airport System API

A .NET 8 Web API for airplane passenger check-in and flight information management using Entity Framework Core with SQLite.

## Features

- **Passenger Management**: Create, read, update, and delete passenger records
- **Flight Management**: Manage flight information and status updates
- **Seat Management**: Handle seat assignments and availability
- **Check-in System**: Automated passenger check-in with seat assignment

## Models

### Passenger
- `PassengerID` (int, Primary Key)
- `FullName` (string)
- `PassportNumber` (string, unique)
- `FlightID` (int, Foreign Key)
- `AssignedSeatID` (int?, Foreign Key)
- `IsCheckedIn` (bool)

### Flight
- `FlightID` (int, Primary Key)
- `FlightNumber` (string)
- `ArrivalAirport` (string)
- `DestinationAirport` (string)
- `Time` (DateTime)
- `Gate` (string)
- `FlightStatus` (enum: CheckingIn, Boarding, Departed, Delayed, Cancelled)

### Seat
- `SeatID` (int, Primary Key)
- `FlightID` (int, Foreign Key)
- `SeatNumber` (string)
- `IsOccupied` (bool)

## API Endpoints

### Flights
- `GET /api/flights` - Get all flights
- `GET /api/flights/{id}` - Get flight by ID
- `POST /api/flights` - Create new flight
- `PUT /api/flights/{id}` - Update flight
- `PUT /api/flights/{id}/status` - Update flight status
- `DELETE /api/flights/{id}` - Delete flight

### Passengers
- `GET /api/passengers` - Get all passengers
- `GET /api/passengers/{id}` - Get passenger by ID
- `POST /api/passengers` - Create new passenger
- `PUT /api/passengers/{id}` - Update passenger
- `DELETE /api/passengers/{id}` - Delete passenger

### Seats
- `GET /api/seats` - Get all seats
- `GET /api/seats/{id}` - Get seat by ID
- `GET /api/seats/flight/{flightId}` - Get seats by flight
- `POST /api/seats` - Create new seat
- `PUT /api/seats/{id}` - Update seat
- `DELETE /api/seats/{id}` - Delete seat

### Check-in
- `POST /api/checkin` - Check in passenger by passport number

## Running the Application

1. Navigate to the project directory:
   ```bash
   cd AirportSystem
   ```

2. Restore packages:
   ```bash
   dotnet restore
   ```

3. Build the project:
   ```bash
   dotnet build
   ```

4. Run the application:
   ```bash
   dotnet run
   ```

5. Open your browser and navigate to:
   - Swagger UI: `https://localhost:7xxx/swagger` (port will be shown in console)
   - API Base URL: `https://localhost:7xxx/api`

## Database

The application uses SQLite database (`airport.db`) with Entity Framework Core. The database is automatically created when the application starts and includes sample data.

## Sample Data

The application includes pre-seeded data:
- 2 sample flights
- 9 sample seats across both flights
- 3 sample passengers (2 checked in, 1 not checked in)

## Example Usage

### Check-in a passenger:
```bash
POST /api/checkin
Content-Type: application/json

{
  "passportNumber": "P3456789"
}
```

### Update flight status:
```bash
PUT /api/flights/1/status
Content-Type: application/json

{
  "flightStatus": "Boarding"
}
```

### Get all flights:
```bash
GET /api/flights
```
