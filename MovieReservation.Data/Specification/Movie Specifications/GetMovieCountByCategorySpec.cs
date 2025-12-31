using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Data.Specification.Movie_Specifications
{
    public class GetMovieCountByCategorySpec : BaseSpecification<Entities.Movie>
    {
        public GetMovieCountByCategorySpec(int categoryId) 
            : base(m => m.CategoryId == categoryId)
        {
        }
    }
}
