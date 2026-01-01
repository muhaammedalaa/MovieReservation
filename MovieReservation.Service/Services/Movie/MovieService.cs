using AutoMapper;
using Microsoft.Extensions.Logging;
using MovieReservation.Data.Contracts;
using MovieReservation.Data.Dtos;
using MovieReservation.Data.Service.Contract;
using MovieReservation.Data.Specification.Movie_Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Service.Services.Movie
{
    public class MovieService : IMovieService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly ILogger<MovieService> _logger;

        public MovieService(IMapper mapper, IUnitOfWork unitOfWork, ICacheService cacheService, ILogger<MovieService> logger)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _logger = logger;
        }
        public async Task<MovieDTO> CreateMovieAsync(MovieDTO movieDTO)
        {
            if (movieDTO == null)
                throw new ArgumentNullException(nameof(movieDTO));

            ValidateMovieDTO(movieDTO);
            if (movieDTO.Category?.Id > 0)
            {
                var categoryExists = await CategoryExistsAsync(movieDTO.Category.Id);
                if (!categoryExists)
                    throw new ArgumentException("Specified category does not exist", nameof(movieDTO.Category.Id));
            }
            var movieEntity = _mapper.Map<Data.Entities.Movie>(movieDTO);
            movieEntity.CreatedAt = DateTime.UtcNow;
            await _unitOfWork.Repository<Data.Entities.Movie>().AddAsync(movieEntity);
            await _unitOfWork.SaveChangesAsync();
            await _cacheService.RemoveByPatternAsync("Movie:*"); // Invalidate all movie-related caches
            _logger.LogInformation("🗑️ Cache invalidated for pattern 'Movie:*' after creating movie: {MovieTitle}", movieEntity.Titel);
            return _mapper.Map<MovieDTO>(movieEntity);
        }

        public async Task<bool> DeleteMovieAsync(int movieId)
        {
            if (movieId <= 0)
                throw new ArgumentException("Movie ID must be greater than zero.", nameof(movieId));
            var movieTask = await _unitOfWork.Repository<Data.Entities.Movie>().GetByIdAsync(movieId);
            if (movieTask == null)
                return false;
            _unitOfWork.Repository<Data.Entities.Movie>().Delete(movieTask);
            await _unitOfWork.SaveChangesAsync();
            await _cacheService.RemoveByPatternAsync("Movie:*"); // Invalidate all movie-related caches
            _logger.LogInformation("🗑️ Cache invalidated for pattern 'Movie:*' after deleting movie ID: {MovieId}", movieId);
            return true;
        }

        public async Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync()
        {
            var categories = await _unitOfWork.Repository<Data.Entities.Category>().GetAllAsync();
            return _mapper.Map<IEnumerable<CategoryDTO>>(categories);
        }

        public async Task<PaginatedResultDTO<MovieDTO>> GetAllMoviesAsync(int pageNumber = 1, int pageSize = 10)
        {
            ValidatePagingParameters(pageNumber, pageSize);
            var cacheKey = $"Movie:page:{pageNumber}:size:{pageSize}";
            try
            {
                // Try to get from cache
                var cachedResult = await _cacheService.GetAsync<PaginatedResultDTO<MovieDTO>>(cacheKey);
                if (cachedResult != null)
                {
                    _logger.LogInformation(
                        "✅ Cache HIT: Retrieved all movies from cache (Page: {PageNumber}, Size: {PageSize})",
                        pageNumber, pageSize);
                    return cachedResult;
                }

                _logger.LogInformation(
                    "❌ Cache MISS: Fetching all movies from database (Page: {PageNumber}, Size: {PageSize})",
                    pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache retrieval failed, proceeding to database");
            }
            var spec = new GetMoviesWithCategorySpec(pageNumber, pageSize);
            var movies = await _unitOfWork.Repository<Data.Entities.Movie>().GetAsync(spec);
            var countSpec = new GetMovieCountSpec();
            var totalItems = await _unitOfWork.Repository<Data.Entities.Movie>().CountAsync(countSpec);
            var mappedMovies = _mapper.Map<IEnumerable<MovieDTO>>(movies);
            var result = new PaginatedResultDTO<MovieDTO>
            {
                Items = mappedMovies.ToList(),
                TotalCount = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            int cacheMinutes = 5;
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(cacheMinutes));
            _logger.LogInformation(
        "📝 Cached all movies (Page: {PageNumber}, Size: {PageSize}) for {Minutes} minutes",
        pageNumber, pageSize, cacheMinutes);
            return result;

        }

        public async Task<MovieDetailDTO> GetMovieDetailAsync(int movieId)
        {
            if (movieId <= 0)
                throw new ArgumentException("Movie ID must be greater than zero.", nameof(movieId));
            var spec = new GetMovieDetailsSpec(movieId);
            var movie = await _unitOfWork.Repository<Data.Entities.Movie>().GetSingleAsync(spec);
            if (movie == null)
                return null;
            return _mapper.Map<MovieDetailDTO>(movie);

        }

        public async Task<PaginatedResultDTO<MovieDTO>> GetMoviesByAgeAsync(int age, int pageNumber = 1, int pageSize = 10)
        {
            ValidatePagingParameters(pageNumber, pageSize);
            if (age < 0)
            {
                throw new ArgumentException("Age must be a non-negative integer.", nameof(age));
            }
            var spec = new GetMoviesByAgeSpec(age, pageNumber, pageSize);
            var moviesTask = await _unitOfWork.Repository<Data.Entities.Movie>().GetAsync(spec);
            var countSpec = new GetMovieCountSpec();
            var totalItems = await _unitOfWork.Repository<Data.Entities.Movie>().CountAsync(countSpec);
            var mappedMovies = _mapper.Map<IEnumerable<MovieDTO>>(moviesTask);
            return new PaginatedResultDTO<MovieDTO>
            {
                Items = mappedMovies.ToList(),
                TotalCount = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<PaginatedResultDTO<MovieDTO>> GetMoviesByCategoryAsync(int categoryId, int pageNumber = 1, int pageSize = 10)
        {
            ValidatePagingParameters(pageNumber, pageSize);
            if (categoryId <= 0)
                throw new ArgumentException("Category ID must be greater than zero.", nameof(categoryId));
            var categoryExists = await CategoryExistsAsync(categoryId);
            var spec = new GetMoviesByCategorySpec(categoryId, pageNumber, pageSize);

            var movies = await _unitOfWork.Repository<Data.Entities.Movie>().GetAsync(spec);

            var countSpec = new GetMovieCountByCategorySpec(categoryId);

            var totalItems = await _unitOfWork.Repository<Data.Entities.Movie>().CountAsync(countSpec);
            var mappedMovies = _mapper.Map<IEnumerable<MovieDTO>>(movies);
            return new PaginatedResultDTO<MovieDTO>
            {
                Items = mappedMovies.ToList(),
                TotalCount = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<bool> MovieExistsAsync(int movieId)
        {
            if (movieId <= 0)
                return false;
            var movie = await _unitOfWork.Repository<Data.Entities.Movie>().GetByIdAsync(movieId);
            return movie != null;

        }

        public async Task<PaginatedResultDTO<MovieDTO>> SearchMoviesAsync(string searchTerm, int pageSize = 10, int pageNumber = 1)
        {
            ValidatePagingParameters(pageNumber, pageSize);
            if (string.IsNullOrWhiteSpace(searchTerm))
                throw new ArgumentException("Search term cannot be empty", nameof(searchTerm));
            if (searchTerm.Length > 100)
                throw new ArgumentException("Search term is too long", nameof(searchTerm));
            var spec = new SearchMoviesSpec(searchTerm, pageNumber, pageSize);
            var moviesTask = await _unitOfWork.Repository<Data.Entities.Movie>().GetAsync(spec);
            var totalCount = await _unitOfWork.Repository<Data.Entities.Movie>().CountAsync(spec);
            var mappedMovies = _mapper.Map<IEnumerable<MovieDTO>>(moviesTask);
            return new PaginatedResultDTO<MovieDTO>
            {
                Items = mappedMovies.ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<bool> UpdateMovieAsync(MovieDTO movieDTO, int movieId)
        {
            if (movieDTO == null)
                throw new ArgumentNullException(nameof(movieDTO));
            if (movieId <= 0)
                throw new ArgumentException("Movie ID must be greater than zero.", nameof(movieId));
            ValidateMovieDTO(movieDTO);
            var existingMovieTask = await _unitOfWork.Repository<Data.Entities.Movie>().GetByIdAsync(movieId);
            if (existingMovieTask == null)
                return false;
            if (movieDTO.Category?.Id > 0 && movieDTO.Category.Id != existingMovieTask.CategoryId)
            {
                var categoryExists = await CategoryExistsAsync(movieDTO.Category.Id);
                if (!categoryExists)
                    throw new ArgumentException("Specified category does not exist", nameof(movieDTO.Category.Id));
                existingMovieTask.CategoryId = movieDTO.Category.Id;
            }
            _mapper.Map(movieDTO, existingMovieTask);
            existingMovieTask.Id = movieId;
            _unitOfWork.Repository<Data.Entities.Movie>().Update(existingMovieTask);
            await _unitOfWork.SaveChangesAsync();
            await _cacheService.RemoveByPatternAsync("Movie:*"); // Invalidate all movie-related caches
            _logger.LogInformation("🗑️ Cache invalidated for pattern 'Movie:*' after updating movie ID: {MovieId}", movieId);
            return true;
        }
        private static void ValidatePagingParameters(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0)
            {
                throw new ArgumentException("Page number must be greater than zero.", nameof(pageNumber));
            }
            if (pageSize <= 0 || pageSize > 100)
            {
                throw new ArgumentException("Page size must be greater than zero.", nameof(pageSize));
            }
        }
        private async Task<bool> CategoryExistsAsync(int categoryId)
        {
            var category = await _unitOfWork.Repository<Data.Entities.Category>().GetByIdAsync(categoryId);
            return category != null;
        }
        private static void ValidateMovieDTO(MovieDTO movieDTO)
        {
            if (movieDTO == null)
                throw new ArgumentNullException(nameof(movieDTO));
            if (string.IsNullOrWhiteSpace(movieDTO.Title))
                throw new ArgumentException("Movie title cannot be empty", nameof(movieDTO.Title));
            if (movieDTO.SuitableAge < 0)
                throw new ArgumentException("Age restriction must be a non-negative integer", nameof(movieDTO.SuitableAge));
            if (movieDTO.DurationInMinutes <= 0)
                throw new ArgumentException("Duration must be a positive TimeSpan", nameof(movieDTO.DurationInMinutes));
        }
    }
}
