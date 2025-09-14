# Deployment Guide

## Overview

This guide covers deploying the Airport Management System to various environments, from development to production.

## Prerequisites

- .NET 8 Runtime
- SQLite (or SQL Server for production)
- Web server (IIS, Nginx, Apache)
- SSL certificate (for production)

## Development Deployment

### Local Development

1. **Clone and setup:**
   ```bash
   git clone <repository-url>
   cd APS
   dotnet restore
   ```

2. **Run the API:**
   ```bash
   cd AirportSystem
   dotnet run
   ```

3. **Run the Blazor app:**
   ```bash
   cd AirportSystemBlazor
   dotnet run
   ```

4. **Access applications:**
   - API: `https://localhost:7xxx`
   - Blazor: `https://localhost:5xxx`

## Production Deployment

### Option 1: Self-Hosted

#### 1. Publish Applications

**API:**
```bash
cd AirportSystem
dotnet publish -c Release -o ./publish
```

**Blazor:**
```bash
cd AirportSystemBlazor
dotnet publish -c Release -o ./publish
```

#### 2. Configure Production Settings

**appsettings.Production.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=/var/app/airport.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "yourdomain.com"
}
```

#### 3. Set up as Windows Service

**Create service script:**
```powershell
sc create "AirportAPI" binPath="C:\path\to\AirportSystem.exe" start=auto
sc start "AirportAPI"
```

#### 4. Configure Reverse Proxy (Nginx)

**nginx.conf:**
```nginx
server {
    listen 80;
    server_name yourdomain.com;
    
    location /api/ {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }
    
    location / {
        proxy_pass http://localhost:5001;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }
}
```

### Option 2: Docker Deployment

#### 1. Create Dockerfile

**AirportSystem/Dockerfile:**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AirportSystem/AirportSystem.csproj", "AirportSystem/"]
RUN dotnet restore "AirportSystem/AirportSystem.csproj"
COPY . .
WORKDIR "/src/AirportSystem"
RUN dotnet build "AirportSystem.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AirportSystem.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AirportSystem.dll"]
```

**AirportSystemBlazor/Dockerfile:**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AirportSystemBlazor/AirportSystemBlazor.csproj", "AirportSystemBlazor/"]
RUN dotnet restore "AirportSystemBlazor/AirportSystemBlazor.csproj"
COPY . .
WORKDIR "/src/AirportSystemBlazor"
RUN dotnet build "AirportSystemBlazor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AirportSystemBlazor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AirportSystemBlazor.dll"]
```

#### 2. Create Docker Compose

**docker-compose.yml:**
```yaml
version: '3.8'

services:
  airport-api:
    build:
      context: .
      dockerfile: AirportSystem/Dockerfile
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Data Source=/app/data/airport.db
    volumes:
      - airport-data:/app/data
    restart: unless-stopped

  airport-blazor:
    build:
      context: .
      dockerfile: AirportSystemBlazor/Dockerfile
    ports:
      - "5001:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
    depends_on:
      - airport-api
    restart: unless-stopped

volumes:
  airport-data:
```

#### 3. Deploy with Docker

```bash
docker-compose up -d
```

### Option 3: Cloud Deployment

#### Azure App Service

1. **Create App Service Plans:**
   - API: App Service Plan for API
   - Blazor: App Service Plan for Web App

2. **Configure Application Settings:**
   ```json
   {
     "ConnectionStrings:DefaultConnection": "Data Source=D:\\home\\site\\wwwroot\\airport.db",
     "ASPNETCORE_ENVIRONMENT": "Production"
   }
   ```

3. **Deploy via Azure DevOps or GitHub Actions**

#### AWS Elastic Beanstalk

1. **Create Elastic Beanstalk Applications:**
   - API: .NET Core application
   - Blazor: .NET Core application

2. **Configure Environment Variables:**
   ```
   ASPNETCORE_ENVIRONMENT=Production
   ConnectionStrings__DefaultConnection=Data Source=airport.db
   ```

3. **Deploy using EB CLI:**
   ```bash
   eb init
   eb create production
   eb deploy
   ```

## Database Configuration

### SQLite (Default)

**Advantages:**
- Zero configuration
- File-based
- Perfect for development and small deployments

**Configuration:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=airport.db"
  }
}
```

### SQL Server (Production Recommended)

**Advantages:**
- Better performance
- Advanced features
- Better for high-traffic scenarios

**Configuration:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=server;Database=AirportDB;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

**Migration:**
1. Install SQL Server provider:
   ```bash
   dotnet add package Microsoft.EntityFrameworkCore.SqlServer
   ```

2. Update DbContext configuration:
   ```csharp
   options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
   ```

3. Run migrations:
   ```bash
   dotnet ef database update
   ```

## Security Configuration

### HTTPS Setup

1. **Obtain SSL Certificate:**
   - Let's Encrypt (free)
   - Commercial certificate
   - Self-signed (development only)

2. **Configure Kestrel:**
   ```json
   {
     "Kestrel": {
       "Endpoints": {
         "HttpsInlineCertFile": {
           "Url": "https://localhost:5001",
           "Certificate": {
             "Path": "path/to/certificate.pfx",
             "Password": "certificate-password"
           }
         }
       }
     }
   }
   ```

### CORS Configuration

**Production CORS:**
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins("https://yourdomain.com")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

### Authentication (Future Enhancement)

Consider implementing:
- JWT Bearer tokens
- OAuth 2.0
- Azure AD integration
- API key authentication

## Monitoring and Logging

### Application Insights (Azure)

1. **Add Application Insights:**
   ```bash
   dotnet add package Microsoft.ApplicationInsights.AspNetCore
   ```

2. **Configure in Program.cs:**
   ```csharp
   builder.Services.AddApplicationInsightsTelemetry();
   ```

### Serilog (Alternative)

1. **Add Serilog:**
   ```bash
   dotnet add package Serilog.AspNetCore
   dotnet add package Serilog.Sinks.File
   ```

2. **Configure logging:**
   ```csharp
   builder.Host.UseSerilog((context, configuration) =>
       configuration.ReadFrom.Configuration(context.Configuration));
   ```

## Performance Optimization

### Caching

1. **Add Memory Cache:**
   ```csharp
   builder.Services.AddMemoryCache();
   ```

2. **Add Response Caching:**
   ```csharp
   builder.Services.AddResponseCaching();
   app.UseResponseCaching();
   ```

### Database Optimization

1. **Connection Pooling:**
   ```csharp
   builder.Services.AddDbContext<AirportDbContext>(options =>
       options.UseSqlServer(connectionString, sqlOptions =>
       {
           sqlOptions.EnableRetryOnFailure();
           sqlOptions.CommandTimeout(30);
       }));
   ```

2. **Query Optimization:**
   - Use `AsNoTracking()` for read-only queries
   - Implement pagination for large datasets
   - Use proper indexing

## Health Checks

1. **Add Health Checks:**
   ```csharp
   builder.Services.AddHealthChecks()
       .AddDbContext<AirportDbContext>();
   ```

2. **Configure endpoints:**
   ```csharp
   app.MapHealthChecks("/health");
   ```

## Backup Strategy

### Database Backup

**SQLite:**
```bash
# Simple file copy
cp airport.db airport_backup_$(date +%Y%m%d).db
```

**SQL Server:**
```sql
BACKUP DATABASE AirportDB TO DISK = 'C:\Backups\AirportDB.bak'
```

### Automated Backups

**Windows Task Scheduler:**
```powershell
# Create backup task
schtasks /create /tn "AirportDB Backup" /tr "powershell -file backup.ps1" /sc daily /st 02:00
```

## Troubleshooting

### Common Issues

1. **Port Conflicts:**
   - Check port usage: `netstat -an | findstr :5000`
   - Update launchSettings.json

2. **Database Locked:**
   - Ensure only one instance is running
   - Check file permissions

3. **SignalR Connection Issues:**
   - Verify CORS configuration
   - Check firewall settings
   - Ensure WebSocket support

### Logging

Enable detailed logging:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

## Maintenance

### Regular Tasks

1. **Database Maintenance:**
   - Regular backups
   - Index optimization
   - Cleanup old data

2. **Application Updates:**
   - Security patches
   - Feature updates
   - Performance improvements

3. **Monitoring:**
   - Check application health
   - Monitor performance metrics
   - Review error logs

### Scaling Considerations

1. **Horizontal Scaling:**
   - Load balancer configuration
   - Session state management
   - Database clustering

2. **Vertical Scaling:**
   - Increase server resources
   - Optimize application performance
   - Database tuning

## Support

For deployment issues:
1. Check application logs
2. Verify configuration settings
3. Test connectivity
4. Review security settings
5. Contact support team
