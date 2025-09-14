namespace AirportSystem.Models
{
    /// <summary>
    /// Represents the possible status values for a flight in the airport management system.
    /// </summary>
    public enum FlightStatus
    {
        /// <summary>
        /// Flight is currently accepting passenger check-ins.
        /// </summary>
        CheckingIn,

        /// <summary>
        /// Flight is in the boarding process.
        /// </summary>
        Boarding,

        /// <summary>
        /// Flight has departed from the gate.
        /// </summary>
        Departed,

        /// <summary>
        /// Flight departure has been delayed.
        /// </summary>
        Delayed,

        /// <summary>
        /// Flight has been cancelled.
        /// </summary>
        Cancelled
    }
}
