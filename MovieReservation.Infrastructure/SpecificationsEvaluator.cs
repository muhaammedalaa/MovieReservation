using Microsoft.EntityFrameworkCore;
using MovieReservation.Data.Specification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Infrastructure
{
    public class SpecificationsEvaluator<T> where T : class
    {
        public static IQueryable<T> GetQuery(IQueryable<T> inputQuery, ISpecification<T> specification)
        {
            var query = inputQuery;
            // Apply criteria
            if (specification.Criteria is not null)
            {
                query = query.Where(specification.Criteria);
            }
            // Apply includes
            query = specification.Includes.Aggregate(query, (current, include) => current.Include(include));
            query = specification.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));
            // Apply ordering
            if (specification.OrderBy is not null)
            {
                query = query.OrderBy(specification.OrderBy);
            }
            else if (specification.OrderByDescending is not null)
            {
                query = query.OrderByDescending(specification.OrderByDescending);
            }
            // Apply paging
            if (specification.IsPagingEnabled)
            {
                if (specification.Skip.HasValue)
                {
                    query = query.Skip(specification.Skip.Value);
                }
                if (specification.Take.HasValue)
                {
                    query = query.Take(specification.Take.Value);
                }
            }
            return query;
        }
    }
}
