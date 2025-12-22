using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Data.Specification.Movie_Specifications
{
    public class GetMoviesByCategorySpec : BaseSpecification<Entities.Movie>
    {
        public GetMoviesByCategorySpec(int categoryId, int PageNumber, int PageSize)
            : base(m => m.CategoryId == categoryId)
        {
            AddInclude(m => m.Category);
            AddOrderByDescending(m => m.CreatedAt);
            ApplyPaging((PageNumber - 1) * PageSize, PageSize);
        }
    }
}
