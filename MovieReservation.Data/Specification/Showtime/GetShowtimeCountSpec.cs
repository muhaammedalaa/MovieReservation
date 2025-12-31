using MovieReservation.Data.Specification;

namespace MovieReservation.Data.Specification.Showtime_Specifications
{
    /// <summary>
    /// Specification for counting showtimes (for pagination totals)
    /// </summary>
    public class GetShowtimeCountSpec : BaseSpecification<Entities.Showtime>
    {
        public GetShowtimeCountSpec()
            : base(s => s.StartDate >= System.DateTime.UtcNow)
        {
        }
    }
}