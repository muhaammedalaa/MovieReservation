using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Data.Specification.Movie_Specifications
{
    public class GetMoviesWithCategorySpec : BaseSpecification<Entities.Movie>
    {
        public GetMoviesWithCategorySpec() : base()
        {
            AddInclude(m => m.Category);
            AddOrderBy(m => m.CreatedAt);
        }
        public GetMoviesWithCategorySpec(int PageNumber, int PageSize) : base()
        {
            AddInclude(m => m.Category);
            AddOrderByDescending(m => m.CreatedAt);
            ApplyPaging((PageNumber - 1) * PageSize, PageSize);
        }

    }
}
