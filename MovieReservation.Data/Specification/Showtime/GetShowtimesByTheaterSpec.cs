using MovieReservation.Data.Specification;

namespace MovieReservation.Data.Specification.Showtime_Specifications
{
    /// <summary>
    /// Specification for retrieving showtimes by theater with pagination
    /// </summary>
    public class GetShowtimesByTheaterSpec : BaseSpecification<Entities.Showtime>
    {
        public GetShowtimesByTheaterSpec(int theaterId, int pageNumber, int pageSize)
            : base(s => s.TheaterId == theaterId && s.StartDate >= System.DateTime.UtcNow)
        {
            AddInclude(s => s.Movie);
            AddInclude(s => s.Theater);
            AddOrderBy(s => s.StartDate);
            ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        }
    }
}