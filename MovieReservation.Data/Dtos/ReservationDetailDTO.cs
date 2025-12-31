using System;

namespace MovieReservation.Data.Dtos
{
    /// <summary>
    /// Detailed DTO for reservation information with full showtime and movie details
    /// </summary>
    public class ReservationDetailDTO
    {
        public int Id { get; set; }
        public int SeatNumber { get; set; }
        public string SecretCode { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Showtime Details
        public int ShowtimeId { get; set; }
        public decimal Price { get; set; }
        public DateTime ShowDateTime { get; set; }
        
        // Movie Details
        public string MovieTitle { get; set; }
        public string MoviePoster { get; set; }
        public int DurationInMinutes { get; set; }
        
        // Theater Details
        public string TheaterName { get; set; }
        public int TotalSeats { get; set; }
    }
}