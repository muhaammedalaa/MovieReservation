using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Data.Specification.Movie_Specifications
{
    public class SearchMoviesSpec : BaseSpecification<Entities.Movie>
    {
        public SearchMoviesSpec(string searchTerm, int PageNumber, int PageSize)
            : base(m => m.Titel.Contains(searchTerm) || m.Description.Contains(searchTerm))
        {
            AddInclude(m => m.Category);
            AddOrderByDescending(m => m.CreatedAt);
            ApplyPaging((PageNumber - 1) * PageSize, PageSize);
        }
    }
}
