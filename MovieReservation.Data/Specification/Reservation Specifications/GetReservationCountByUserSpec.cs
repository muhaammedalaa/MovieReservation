using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Data.Specification.Reservation_Specifications
{
    public class GetReservationCountByUserSpec : BaseSpecification<Entities.Reservation>
    {
        public GetReservationCountByUserSpec(string userId)
            : base(r => r.AppUserId == userId)
        {
        }
    }
}
