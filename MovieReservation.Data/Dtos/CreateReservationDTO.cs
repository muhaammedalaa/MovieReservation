using System;

namespace MovieReservation.Data.Dtos
{
    /// <summary>
    /// DTO for creating a new reservation
    /// </summary>
    public class CreateReservationDTO
    {
        public int ShowtimeId { get; set; }
        public int SeatNumber { get; set; }
    }
}