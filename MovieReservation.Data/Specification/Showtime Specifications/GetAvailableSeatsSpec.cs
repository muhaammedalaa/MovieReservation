using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Data.Specification.Showtime_Specifications
{
    public class GetAvailableSeatsSpec : BaseSpecification<Entities.Showtime>
    {
        public GetAvailableSeatsSpec(int showtimeId)
            : base(s => s.Id == showtimeId)
        {
            AddInclude(s => s.Theater);
            AddInclude(s => s.Reservations);
        }
    }
}
