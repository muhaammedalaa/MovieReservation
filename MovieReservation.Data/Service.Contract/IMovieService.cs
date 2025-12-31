using MovieReservation.Data.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Data.Service.Contract
{
    public interface IMovieService
    {
        Task<PaginatedResultDTO<MovieDTO>> GetAllMoviesAsync(int pageNumber = 1, int pageSize = 10);
        Task<MovieDetailDTO> GetMovieDetailAsync(int movieId);
        Task<PaginatedResultDTO<MovieDTO>> GetMoviesByCategoryAsync(int categoryId, int pageNumber = 1, int pageSize = 10);
        Task<PaginatedResultDTO<MovieDTO>> SearchMoviesAsync(string searchTerm, int pageSize = 10, int pageNumber = 1);
        Task<bool> MovieExistsAsync(int movieId);
        Task<PaginatedResultDTO<MovieDTO>> GetMoviesByAgeAsync(int age, int pageNumber = 1, int pageSize = 10);
        Task<MovieDTO> CreateMovieAsync(MovieDTO movieDTO);
        Task<bool> UpdateMovieAsync(MovieDTO movieDTO, int movieId);
        Task<bool> DeleteMovieAsync(int movieId);
        Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync();
    }
}
