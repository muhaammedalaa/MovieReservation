using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Data.Specification.Reservation_Specifications
{
    public class GetReservationsByShowtimeSpec : BaseSpecification<Entities.Reservation>
    {
        public GetReservationsByShowtimeSpec(int showtimeId)
            : base(r => r.ShowTimeId == showtimeId)
        {
        }
    }
}
