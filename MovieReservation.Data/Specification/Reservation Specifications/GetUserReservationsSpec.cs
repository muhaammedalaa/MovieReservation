using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Data.Specification.Reservation_Specifications
{
    public class GetUserReservationsSpec : BaseSpecification<Entities.Reservation>
    {
        public GetUserReservationsSpec(string userId, int pageNumber, int pageSize)
            : base(r => r.AppUserId == userId)
        {
            AddInclude(r => r.Showtime);
            AddInclude("Showtime.Movie");
            AddInclude("Showtime.Theater");
            AddOrderByDescending(r => r.CreatedAt);
            ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        }
        public GetUserReservationsSpec(string userId)
            : base(r => r.AppUserId == userId)
        {

        }
    }
}
