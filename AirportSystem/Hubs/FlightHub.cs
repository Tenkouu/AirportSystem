using Microsoft.AspNetCore.SignalR;

namespace AirportSystem.Hubs
{
    /// <summary>
    /// SignalR hub for managing flight-related real-time communications.
    /// Handles client connections to flight-specific groups for status updates.
    /// </summary>
    public class FlightHub : Hub
    {
        /// <summary>
        /// Adds a client connection to a specific flight group for receiving updates.
        /// </summary>
        /// <param name="flightId">The ID of the flight to join.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task JoinFlightGroup(int flightId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Flight_{flightId}");
        }

        /// <summary>
        /// Removes a client connection from a specific flight group.
        /// </summary>
        /// <param name="flightId">The ID of the flight to leave.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LeaveFlightGroup(int flightId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Flight_{flightId}");
        }
    }
}
