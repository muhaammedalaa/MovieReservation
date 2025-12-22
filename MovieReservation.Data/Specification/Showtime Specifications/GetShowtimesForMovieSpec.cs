using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Data.Specification.Showtime_Specifications
{
    public class GetShowtimesForMovieSpec : BaseSpecification<Entities.Showtime>
    {
        public GetShowtimesForMovieSpec(int movieId, DateTime date) : base(s => s.MovieId == movieId && s.StartDate.Date == date.Date)
        {
            AddInclude(s => s.Movie);
            AddInclude(s => s.Theater);
            AddInclude(s => s.StartDate);
        }
    }
}
