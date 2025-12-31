using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Data.Dtos
{
    public class ShowtimeDetailDTO
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public DateTime StartDate { get; set; }

        // Movie Details
        public int MovieId { get; set; }
        public string MovieTitle { get; set; }
        public string MovieDescription { get; set; }
        public string MoviePoster { get; set; }
        public int MovieDurationInMinutes { get; set; }
        public int MovieSuitableAge { get; set; }

        // Theater Details
        public int TheaterId { get; set; }
        public string TheaterName { get; set; }
        public int TheaterTotalSeats { get; set; }

        // Availability
        public int AvailableSeats { get; set; }
        public int ReservedSeats { get; set; }
        public List<int> ReservedSeatNumbers { get; set; } = new();
    }
}
