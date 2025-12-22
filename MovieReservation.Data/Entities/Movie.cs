using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Data.Entities
{
    public class Movie
    {
        public int Id { get; set; }
        public string Titel {  get; set; }
        public string Description { get; set; }
        public string Poster { get; set; }
        public int DurationInMinutes { get; set; }
        public int SuitableAge { get; set; }
        public DateOnly releaseDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public Category? Category { get; set; }
        public int ? CategoryId { get; set; }
        public ICollection<Showtime>? Showtimes { get; set; }
    }
}
