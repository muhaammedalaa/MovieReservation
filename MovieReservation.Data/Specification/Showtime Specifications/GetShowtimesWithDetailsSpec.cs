using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Data.Specification.Showtime_Specifications
{
    public class GetShowtimesWithDetailsSpec : BaseSpecification<Entities.Showtime>
    {
        public GetShowtimesWithDetailsSpec(int movieId, int pageNumber, int pageSize)
            : base(s => s.MovieId == movieId && s.StartDate >= DateTime.UtcNow)
        {
            AddInclude(s => s.Movie);
            AddInclude(s => s.Theater);
            AddInclude(s => s.Reservations);
            AddOrderBy(s => s.StartDate);
            ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        }
    }
}
