using System;

namespace MovieReservation.Data.Dtos
{
    /// <summary>
    /// DTO for displaying user's reservations in a list
    /// </summary>
    public class UserReservationDTO
    {
        public int ReservationId { get; set; }
        public int SeatNumber { get; set; }
        public string SecretCode { get; set; }
        public DateTime ReservationDate { get; set; }

        public string MovieTitle { get; set; }
        public string MoviePoster { get; set; }
        public DateTime ShowDateTime { get; set; }
        public decimal TicketPrice { get; set; }

        public string TheaterName { get; set; }
    }
}