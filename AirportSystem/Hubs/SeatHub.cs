using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace AirportSystem.Hubs
{
    /// <summary>
    /// SignalR hub for managing seat-related real-time communications.
    /// Handles seat selection, deselection, and occupancy updates for flight groups.
    /// </summary>
    public class SeatHub : Hub
    {
        /// <summary>
        /// Adds a client connection to a specific flight group for receiving seat updates.
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

        /// <summary>
        /// Notifies other clients in the flight group that a seat has been temporarily selected.
        /// This prevents multiple users from selecting the same seat simultaneously.
        /// </summary>
        /// <param name="flightId">The ID of the flight containing the seat.</param>
        /// <param name="seatNumber">The seat number that has been selected.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SelectSeat(int flightId, string seatNumber)
        {
            await Clients.OthersInGroup($"Flight_{flightId}")
                .SendAsync("SeatSelected", seatNumber);
        }

        /// <summary>
        /// Notifies other clients in the flight group that a seat selection has been cancelled.
        /// This makes the seat available for selection by other users.
        /// </summary>
        /// <param name="flightId">The ID of the flight containing the seat.</param>
        /// <param name="seatNumber">The seat number that has been deselected.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeselectSeat(int flightId, string seatNumber)
        {
            await Clients.OthersInGroup($"Flight_{flightId}")
                .SendAsync("SeatDeselected", seatNumber);
        }
    }
}