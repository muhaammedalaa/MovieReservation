namespace MovieReservation.Data.Dtos
{
    /// <summary>
    /// DTO for seat availability status
    /// </summary>
    public class SeatAvailabilityDTO
    {
        public int ShowtimeId { get; set; }
        public int SeatNumber { get; set; }
        public bool IsAvailable { get; set; }
        public string Message { get; set; }
    }
}