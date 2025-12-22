using MovieReservation.Data.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Data.Entities
{
    public class Reservation
    {
        public int Id { get; set; }
        public int SeatNumber { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string SecretCode { get; set; }
        public int? ShowTimeId { get; set; }
        public string AppUserId { get; set; }
        public Showtime? Showtime { get; set; }
        //public AppUser? User { get; set; }
        
    }
}
