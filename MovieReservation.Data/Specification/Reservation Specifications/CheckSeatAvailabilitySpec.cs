using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Data.Specification.Reservation_Specifications
{
    public class CheckSeatAvailabilitySpec : BaseSpecification<Entities.Reservation>
    {
        public CheckSeatAvailabilitySpec(int showtimeId, int seatNumber)
            : base(r => r.ShowTimeId == showtimeId && r.SeatNumber == seatNumber)
        {

        }
    }
}
