using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Data.Dtos
{
    public class ShowtimeDTO
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public DateTime StartDate { get; set; }
        public int MovieId { get; set; }
        public int TheaterId { get; set; }
        public string TheaterName { get; set; }
        // Movie Details
        public string MovieTitle { get; set; }
        public string MoviePoster { get; set; }
        public int DurationInMinutes { get; set; }

        // Availability Info
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }
        public int ReservedSeats { get; set; }
    }
}
