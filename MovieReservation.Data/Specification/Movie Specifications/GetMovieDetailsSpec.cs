using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Data.Specification.Movie_Specifications
{
    public class GetMovieDetailsSpec : BaseSpecification<Entities.Movie>
    {
        public GetMovieDetailsSpec(int movieId) : base(m => m.Id == movieId)
        {
            AddInclude(m => m.Category);
            AddInclude(m => m.Showtimes);
        }
    }
}
