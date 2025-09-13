using Microsoft.AspNetCore.SignalR;

namespace AirportSystem.Hubs
{
    public class FlightHub : Hub
    {
        public async Task JoinFlightGroup(int flightId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Flight_{flightId}");
        }

        public async Task LeaveFlightGroup(int flightId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Flight_{flightId}");
        }
    }
}
