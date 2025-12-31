using MovieReservation.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Data.Specification.Showtime_Specifications
{
    public class GetShowtimeByIdSpec : BaseSpecification<Showtime>
    {
        public GetShowtimeByIdSpec(int showtimeid) : base(s => s.Id == showtimeid)
        {
            AddInclude(s => s.Movie);
            AddInclude(s => s.Theater);
            AddInclude(s => s.Reservations);

        }
    }
}
