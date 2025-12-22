using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Data.Specification.Reservation_Specifications
{
    public class VerifyReservationSpec : BaseSpecification<Entities.Reservation>
    {
        public VerifyReservationSpec(int reservationId, string secretCode)
            : base(r => r.Id == reservationId && r.SecretCode == secretCode)
        {
            AddInclude(r => r.Showtime);
            AddInclude("Showtime.Movie");
            AddInclude("Showtime.Theater");

        }
    }
}
