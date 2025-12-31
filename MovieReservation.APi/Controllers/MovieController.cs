using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MovieReservation.Data.Dtos;
using MovieReservation.Data.Service.Contract;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;


namespace MovieReservation.APi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class MovieController : ControllerBase
    {
        private readonly IMovieService _movieService;
        private readonly ILogger<MovieController> _logger;

        public MovieController(IMovieService movieService, ILogger<MovieController> logger)
        {
            _movieService = movieService;
            _logger = logger;
        }
        [HttpGet("GetAllMovies")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResultDTO<MovieDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PaginatedResultDTO<MovieDTO>>>> GetAllMovies([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var movies = await _movieService.GetAllMoviesAsync(pageNumber, pageSize);
                return Ok(new ApiResponse<PaginatedResultDTO<MovieDTO>>(movies, "Movies retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving movies.");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse("An error occurred while processing your request."));
            }

        }
        [HttpGet("GetMovieById/{id}")]
        [ProducesResponseType(typeof(ApiResponse<MovieDetailDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<MovieDetailDTO>>> GetMovieById(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new ApiResponse("Invalid movie ID."));
                var movie = await _movieService.GetMovieDetailAsync(id);
                if (movie == null)
                    return NotFound(new ApiResponse("Movie not found."));
                return Ok(new ApiResponse<MovieDetailDTO>(movie, "Movie retrieved successfully"));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the movie with ID {MovieId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse("An error occurred while processing your request."));
            }

        }
        [HttpGet("SearchMovies")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResultDTO<MovieDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PaginatedResultDTO<MovieDTO>>>> SearchMovies([FromQuery] string searchTerm, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return BadRequest(new ApiResponse("Search term cannot be empty."));
                var movies = await _movieService.SearchMoviesAsync(searchTerm, pageNumber, pageSize);
                if (movies.Items.Count == 0)
                    return NotFound(new ApiResponse("No movies found matching the search criteria."));
                return Ok(new ApiResponse<PaginatedResultDTO<MovieDTO>>(movies, "Movies retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while searching for movies with term '{SearchTerm}'.", searchTerm);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse("An error occurred while processing your request."));
            }
        }
        [HttpGet("category/{categoryId}")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResultDTO<MovieDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PaginatedResultDTO<MovieDTO>>>> GetMoviesByCategory(int categoryId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (categoryId <= 0)
                    return BadRequest(new ApiResponse("Invalid category ID."));
                var movies = await _movieService.GetMoviesByCategoryAsync(categoryId, pageNumber, pageSize);
                if (movies.Items.Count == 0)
                    return NotFound(new ApiResponse("No movies found in the specified category."));
                return Ok(new ApiResponse<PaginatedResultDTO<MovieDTO>>(movies, "Movies retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving movies for category ID {CategoryId}.", categoryId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse("An error occurred while processing your request."));
            }
        }
        [HttpGet("Age /{age}")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResultDTO<MovieDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PaginatedResultDTO<MovieDTO>>>> GetMoviesByAgeRating(int age, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (age < 0)
                    return BadRequest(new ApiResponse("Invalid age rating."));
                var movies = await _movieService.GetMoviesByAgeAsync(age, pageNumber, pageSize);
                if (movies.Items.Count == 0)
                    return NotFound(new ApiResponse("No movies found for the specified age rating."));
                return Ok(new ApiResponse<PaginatedResultDTO<MovieDTO>>(movies, "Movies retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving movies for age rating {Age}.", age);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse("An error occurred while processing your request."));
            }
        }
        [HttpGet("Categories")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<CategoryDTO>>>> GetAllCategories()
        {
            try
            {
                var categories = await _movieService.GetAllCategoriesAsync();
                return Ok(new ApiResponse<IEnumerable<CategoryDTO>>(categories, "Categories retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving categories.");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse("An error occurred while processing your request."));
            }
        }
        [HttpPost("AddMovie")]
        [ProducesResponseType(typeof(ApiResponse<MovieDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<MovieDTO>>> AddMovie([FromBody] MovieDTO createMovieDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ApiResponse("Invalid movie data."));
                _logger.LogInformation("Creating new movie: {Title}", createMovieDto.Title);
                var createdMovie = await _movieService.CreateMovieAsync(createMovieDto);
                return CreatedAtAction(nameof(GetMovieById), new { id = createdMovie.Id }, new ApiResponse<MovieDTO>(createdMovie, "Movie created successfully"));

            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized attempt to create a movie: {Title}", createMovieDto.Title);
                return Unauthorized(new ApiResponse("You are not authorized to perform this action."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a new movie: {Title}", createMovieDto.Title);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse("An error occurred while processing your request."));
            }
        }
        [HttpPut("UpdateMovie/{id}")]
        [ProducesResponseType(typeof(ApiResponse<MovieDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateMovie([FromBody] MovieDTO updateMovieDto, int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new ApiResponse("Invalid movie ID."));
                if (!ModelState.IsValid)
                    return BadRequest(new ApiResponse("Invalid movie data."));
                _logger.LogInformation("Updating movie with ID: {MovieId}", id);
                var updatedMovie = await _movieService.UpdateMovieAsync(updateMovieDto, id);
                if (!updatedMovie)
                {
                    _logger.LogWarning("Movie with ID {MovieId} not found for update", id);
                    return NotFound(new ApiResponse($"Movie with ID {id} not found"));
                }
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized attempt to update movie with ID: {MovieId}", id);
                return Unauthorized(new ApiResponse("You are not authorized to perform this action."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the movie with ID: {MovieId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse("An error occurred while processing your request."));
            }
        }
        [HttpDelete("DeleteMovie/{id}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteMovie(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new ApiResponse("Invalid movie ID."));
                _logger.LogInformation("Deleting movie with ID: {MovieId}", id);
                var deleted = await _movieService.DeleteMovieAsync(id);
                if (!deleted)
                {
                    _logger.LogWarning("Movie with ID {MovieId} not found for deletion", id);
                    return NotFound(new ApiResponse($"Movie with ID {id} not found"));
                }
                return Ok(new ApiResponse("Movie deleted successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized attempt to delete movie with ID: {MovieId}", id);
                return Unauthorized(new ApiResponse("You are not authorized to perform this action."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the movie with ID: {MovieId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse("An error occurred while processing your request."));
            }
        }
        [HttpHead("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> MovieExists(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new ApiResponse("Movie ID must be greater than 0"));

                var exists = await _movieService.MovieExistsAsync(id);

                return exists ? Ok() : NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if movie exists");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse("An error occurred while checking movie existence"));
            }
        }
    }
}

