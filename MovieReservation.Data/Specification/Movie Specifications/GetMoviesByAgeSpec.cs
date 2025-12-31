using MovieReservation.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Data.Specification.Movie_Specifications
{
    public class GetMoviesByAgeSpec : BaseSpecification<Movie>
    {
        public GetMoviesByAgeSpec(int age, int PageNumber, int PageSize)
            : base(m => m.SuitableAge <= age)
        {
            AddInclude(m => m.Category);
            AddOrderByDescending(m => m.CreatedAt);
            ApplyPaging((PageNumber - 1) * PageSize, PageSize);
        }
    }
}
