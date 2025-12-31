namespace MovieReservation.Data.Dtos
{
    /// <summary>
    /// DTO for showtime availability status
    /// </summary>
    public class ShowtimeAvailabilityDTO
    {
        public int ShowtimeId { get; set; }
        public int TotalSeats { get; set; }
        public int ReservedSeats { get; set; }
        public int AvailableSeats { get; set; }
        public decimal OccupancyPercentage { get; set; }
        public bool IsAvailable { get; set; }
    }
}