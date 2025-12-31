using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Data.Dtos
{
    public class MovieDetailDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Poster { get; set; }
        public int DurationInMinutes { get; set; }
        public int SuitableAge { get; set; }
        public DateOnly ReleaseDate { get; set; }
        public CategoryDTO? Category { get; set; }
        public ICollection<ShowtimeDTO>? Showtimes { get; set; } = new List<ShowtimeDTO>();
    }
}
