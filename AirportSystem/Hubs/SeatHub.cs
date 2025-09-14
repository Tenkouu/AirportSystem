using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace AirportSystem.Hubs
{
    public class SeatHub : Hub
    {
        public async Task JoinFlightGroup(int flightId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Flight_{flightId}");
        }


        public async Task LeaveFlightGroup(int flightId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Flight_{flightId}");
        }

        /// <summary>
        /// Called by a client when they click a seat to temporarily lock it for others.
        /// </summary>
        public async Task SelectSeat(int flightId, string seatNumber)
        {
            // Send a message to all OTHER clients in the group that a seat has been temporarily selected
            await Clients.OthersInGroup($"Flight_{flightId}")
                .SendAsync("SeatSelected", seatNumber);
        }

        /// <summary>
        /// Called by a client when they cancel their selection or choose a different seat.
        /// </summary>
        public async Task DeselectSeat(int flightId, string seatNumber)
        {
            // Send a message to all OTHER clients in the group that a seat is now available again
            await Clients.OthersInGroup($"Flight_{flightId}")
                .SendAsync("SeatDeselected", seatNumber);
        }
    }
}