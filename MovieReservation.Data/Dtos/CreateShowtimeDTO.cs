using System;

namespace MovieReservation.Data.Dtos
{
    /// <summary>
    /// DTO for creating a new showtime (Admin only)
    /// </summary>
    public class CreateShowtimeDTO
    {
        public int MovieId { get; set; }
        public int TheaterId { get; set; }
        public decimal Price { get; set; }
        public DateTime StartDate { get; set; }
    }
}