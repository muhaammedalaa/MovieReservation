using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Data.Specification.Reservation_Specifications
{
    public class GetReservationByIdSpec : BaseSpecification<Entities.Reservation>
    {
        public GetReservationByIdSpec(int reservationId)
            : base(r => r.Id == reservationId)
        {
            AddInclude(r => r.Showtime);
            AddInclude("Showtime.Movie");
            AddInclude("Showtime.Theater");
        }
    }
}
