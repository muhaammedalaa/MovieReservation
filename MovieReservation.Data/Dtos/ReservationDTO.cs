using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Data.Dtos
{
    public class ReservationDTO
    {
        public int Id { get; set; }
        public int SeatNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ShowtimeId { get; set; }
        public string AppUserId { get; set; }
        public ShowtimeDTO? Showtime { get; set; }

    }
}
