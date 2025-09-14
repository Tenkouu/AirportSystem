# Airport Management System

A comprehensive airport management system built with .NET 8, featuring real-time flight status updates, passenger check-in, seat management, and a modern web interface.

## 🏗️ Architecture

The system consists of three main components:

- **AirportSystem**: Core Web API with Entity Framework Core and SQLite
- **AirportSystemBlazor**: Real-time web dashboard built with Blazor Server
- **AirportSystemWindows**: Windows desktop application (UWP)

## ✨ Features

### Core Functionality
- **Flight Management**: Complete CRUD operations for flights with real-time status updates
- **Passenger Management**: Passenger registration, check-in, and seat assignment
- **Seat Management**: Dynamic seat allocation with real-time occupancy updates
- **Check-in System**: Automated passenger check-in with optional seat selection
- **Real-time Updates**: SignalR-powered live updates across all connected clients

### Technical Features
- **RESTful API**: Comprehensive API with Swagger documentation
- **Real-time Communication**: SignalR hubs for live flight and seat updates
- **Database Management**: Entity Framework Core with SQLite
- **CORS Support**: Cross-origin resource sharing for web applications
- **Data Validation**: Comprehensive input validation and error handling
- **Modern UI**: Responsive Blazor dashboard with real-time status indicators

## 🚀 Quick Start

### Prerequisites
- .NET 8 SDK
- Visual Studio 2022 or VS Code
- Git

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd APS
   ```

2. **Restore packages**
   ```bash
   dotnet restore
   ```

3. **Build the solution**
   ```bash
   dotnet build
   ```

4. **Run the API**
   ```bash
   cd AirportSystem
   dotnet run
   ```

5. **Run the Blazor Dashboard** (in a new terminal)
   ```bash
   cd AirportSystemBlazor
   dotnet run
   ```

6. **Access the applications**
   - API Swagger UI: `https://localhost:7xxx/swagger`
   - Blazor Dashboard: `https://localhost:5xxx`

## 📊 Database Schema

### Entities

#### Flight
- **FlightID**: Primary key
- **FlightNumber**: Unique flight identifier (max 10 chars)
- **ArrivalAirport**: Departure airport (max 100 chars)
- **DestinationAirport**: Destination airport (max 100 chars)
- **Time**: Scheduled departure time
- **Gate**: Gate number (max 10 chars)
- **FlightStatus**: Enum (CheckingIn, Boarding, Departed, Delayed, Cancelled)

#### Passenger
- **PassengerID**: Primary key
- **FullName**: Passenger's full name (max 100 chars)
- **PassportNumber**: Unique passport identifier (max 20 chars)
- **FlightID**: Foreign key to Flight
- **AssignedSeatID**: Foreign key to Seat (nullable)
- **IsCheckedIn**: Check-in status boolean

#### Seat
- **SeatID**: Primary key
- **FlightID**: Foreign key to Flight
- **SeatNumber**: Seat identifier (max 10 chars)
- **IsOccupied**: Occupancy status boolean
- **PassengerID**: Foreign key to Passenger (nullable)

### Relationships
- Flight → Passengers (One-to-Many)
- Flight → Seats (One-to-Many)
- Passenger → Seat (One-to-One, optional)
- Passenger → Flight (Many-to-One)

## 🔌 API Endpoints

### Flights
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/flights` | Get all flights with seats and passengers |
| GET | `/api/flights/{id}` | Get specific flight by ID |
| POST | `/api/flights` | Create new flight |
| PUT | `/api/flights/{id}` | Update existing flight |
| PUT | `/api/flights/{id}/status` | Update flight status (triggers real-time update) |
| DELETE | `/api/flights/{id}` | Delete flight |

### Passengers
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/passengers` | Get all passengers (supports passport filter) |
| GET | `/api/passengers/{id}` | Get specific passenger by ID |
| POST | `/api/passengers` | Create new passenger |
| PUT | `/api/passengers/{id}` | Update existing passenger |
| DELETE | `/api/passengers/{id}` | Delete passenger |

### Seats
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/seats` | Get all seats with flight and passenger info |
| GET | `/api/seats/{id}` | Get specific seat by ID |
| GET | `/api/seats/flight/{flightId}` | Get all seats for a specific flight |
| POST | `/api/seats` | Create new seat |
| PUT | `/api/seats/{id}` | Update existing seat |
| DELETE | `/api/seats/{id}` | Delete seat |
| POST | `/api/seats/occupy` | Occupy a seat (triggers real-time update) |
| POST | `/api/seats/release` | Release a seat (triggers real-time update) |

### Check-in
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/checkin` | Check in passenger with optional seat selection |

## 🔄 Real-time Features

### SignalR Hubs

#### FlightHub
- **JoinFlightGroup(flightId)**: Subscribe to flight-specific updates
- **LeaveFlightGroup(flightId)**: Unsubscribe from flight updates
- **FlightStatusUpdated**: Broadcasts flight status changes to all clients

#### SeatHub
- **JoinFlightGroup(flightId)**: Subscribe to seat updates for a flight
- **LeaveFlightGroup(flightId)**: Unsubscribe from seat updates
- **SelectSeat(flightId, seatNumber)**: Temporarily lock a seat for selection
- **DeselectSeat(flightId, seatNumber)**: Release a temporarily locked seat
- **SeatOccupied**: Broadcasts when a seat becomes occupied
- **SeatAvailable**: Broadcasts when a seat becomes available

## 📱 User Interface

### Blazor Dashboard
- **Real-time Flight Status**: Live updates of flight statuses
- **Connection Status**: Visual indicator of SignalR connection
- **Responsive Design**: Modern, mobile-friendly interface
- **Status Indicators**: Color-coded flight status badges
- **Auto-refresh**: Automatic data updates without page reload

## 🗄️ Sample Data

The system includes pre-seeded data for testing:

- **10 Flights**: Various international routes with different statuses
- **100 Seats**: 10 seats per flight (1A-5B configuration)
- **33 Passengers**: Distributed across flights, all initially unchecked-in

### Sample Flights
- EK201: Dubai → New York (Checking In)
- QF12: Sydney → Los Angeles (Checking In)
- BA289: London → San Francisco (Boarding)
- LH454: Frankfurt → Chicago (Delayed)
- And 6 more...

## 🛠️ Development

### Project Structure
```
APS/
├── AirportSystem/              # Core Web API
│   ├── Controllers/            # API Controllers
│   ├── Data/                   # Entity Framework Context
│   ├── Hubs/                   # SignalR Hubs
│   ├── Models/                 # Data Models
│   └── Program.cs              # Application Entry Point
├── AirportSystemBlazor/        # Blazor Web Dashboard
│   ├── Pages/                  # Razor Pages
│   └── Program.cs              # Blazor App Entry Point
└── AirportSystemWindows/       # Windows Desktop App
    ├── Pages/                  # XAML Pages
    └── Services/               # API Services
```

### Configuration

#### API Configuration (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=airport.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

#### CORS Configuration
The API is configured to allow requests from:
- `http://0.0.0.0:5001`
- `https://0.0.0.0:5001`

## 🧪 Testing

### API Testing with Swagger
1. Navigate to `https://localhost:7xxx/swagger`
2. Use the interactive API documentation
3. Test endpoints directly from the browser

### Example API Calls

#### Check-in a Passenger
```bash
POST /api/checkin
Content-Type: application/json

{
  "passportNumber": "LSM890123",
  "selectedSeatNumber": "1A"
}
```

#### Update Flight Status
```bash
PUT /api/flights/1/status
Content-Type: application/json

{
  "flightStatus": "Boarding"
}
```

#### Get Available Seats for a Flight
```bash
GET /api/seats/flight/1
```

## 🔧 Troubleshooting

### Common Issues

1. **Database Connection Issues**
   - Ensure SQLite is properly installed
   - Check connection string in appsettings.json
   - Verify database file permissions

2. **SignalR Connection Issues**
   - Check CORS configuration
   - Verify hub URLs in client applications
   - Check firewall settings

3. **Port Conflicts**
   - Update port numbers in launchSettings.json
   - Ensure ports are not in use by other applications

## 📈 Performance Considerations

- **Database Indexing**: Passport numbers are indexed for fast lookups
- **Connection Pooling**: Entity Framework connection pooling enabled
- **Async Operations**: All database operations are asynchronous
- **Real-time Optimization**: SignalR groups for targeted updates

## 🔒 Security Features

- **Input Validation**: Comprehensive data validation on all endpoints
- **SQL Injection Protection**: Entity Framework parameterized queries
- **CORS Configuration**: Controlled cross-origin access
- **Error Handling**: Secure error messages without sensitive data exposure

## 🚀 Deployment

### Production Considerations
1. Update connection strings for production database
2. Configure proper CORS origins
3. Set up HTTPS certificates
4. Configure logging levels
5. Set up monitoring and health checks

### Docker Support
The application can be containerized using Docker:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
COPY . /app
WORKDIR /app
EXPOSE 80
ENTRYPOINT ["dotnet", "AirportSystem.dll"]
```

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📞 Support

For support and questions:
- Create an issue in the repository
- Check the troubleshooting section
- Review the API documentation in Swagger UI

---

**Built with ❤️ using .NET 8, Blazor, and SignalR**
