using MovieReservation.Data.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Data.Service.Contract
{
    public interface IShowtimeService
    {
        // Get all future showtimes with pagination
        Task<PaginatedResultDTO<ShowtimeDTO>> GetAllShowtimesAsync(int pageNumber = 1, int pageSize = 10);
        // Get detailed information about a specific showtime
        Task<ShowtimeDetailDTO> GetShowtimeDetailsAsync(int showtimeId);
        // Get all showtimes for a specific movie with pagination
        Task<PaginatedResultDTO<ShowtimeDTO>> GetShowtimesByMovieAsync(int movieId, int pageNumber = 1, int pageSize = 10);
        // Get showtimes for a movie on a specific date
        Task<IEnumerable<ShowtimeDTO>> GetShowtimesByMovieAndDateAsync(int movieid, DateTime date);
        // Get all showtimes in a specific theater with pagination
        Task<PaginatedResultDTO<ShowtimeDTO>> GetShowtimesByTheaterAsync(int theaterId, int pageNumber = 1, int pageSize = 10);
        // Get showtime availability status
        Task<ShowtimeAvailabilityDTO> GetShowtimeAvailabilityAsync(int showtimeId);
        // Check if a specific seat is available
        Task<IEnumerable<int>> GetReservedSeatsAsync(int showtimeId);
        // Create a new showtime (Admin only)
        Task<ShowtimeDTO> CreateShowtimeAsync(CreateShowtimeDTO createDTO);
        // Update an existing showtime (Admin only)
        Task<bool> UpdateShowtimeAsync(int showtimeId, CreateShowtimeDTO updateDTO);
        // Delete a showtime (Admin only)
        Task<bool> DeleteShowtimeAsync(int showtimeId);
        Task<bool> ShowtimeExistsAsync(int showtimeId);
        Task<IEnumerable<ShowtimeDTO>> GetUpcomingShowtimesAsync(int daysAhead = 7);
        public Task<bool> IsSeatAvailableAsync(int showtimeId, int seatNumber);
    }

}
