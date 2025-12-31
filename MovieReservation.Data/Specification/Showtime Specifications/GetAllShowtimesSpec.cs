using MovieReservation.Data.Entities;
using MovieReservation.Data.Specification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Data.Specification.Showtime_Specifications
{
    public class GetAllShowtimesSpec : BaseSpecification<Showtime>
    {
        public GetAllShowtimesSpec(int pageSize, int pageNumber) : base(S => S.StartDate >= DateTime.UtcNow)
        {
            AddInclude(s => s.Movie);
            AddInclude(s => s.Theater);
            AddOrderBy(s => s.StartDate);
            ApplyPaging((pageNumber - 1) * pageSize, pageSize);

        }
    }
}
