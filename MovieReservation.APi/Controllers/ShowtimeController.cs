using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieReservation.Data.Dtos;
using MovieReservation.Data.Service.Contract;

namespace MovieReservation.APi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ShowtimeController : ControllerBase
    {
        private readonly IShowtimeService _showtimeService;
        private readonly ILogger _logger;

        public ShowtimeController(IShowtimeService showtimeService, ILogger<ShowtimeController> logger)
        {
            _showtimeService = showtimeService;
            _logger = logger;
        }
        // Get all future showtimes
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResultDTO<ShowtimeDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PaginatedResultDTO<ShowtimeDTO>>>> GetAllShowTimes([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Getting all showtimes - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);
                var result = await _showtimeService.GetAllShowtimesAsync(pageNumber, pageSize);
                return Ok(new ApiResponse<PaginatedResultDTO<ShowtimeDTO>>(result,
                    $"Retrieved {result.Items.Count} showtimes successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid pagination parameters: {Message}", ex.Message);
                return BadRequest(new ApiResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all showtimes");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse("An error occurred while retrieving showtimes"));

            }
        }
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ShowtimeDetailDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ShowtimeDetailDTO>>> GetShowtimeDetails(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new ApiResponse("Showtime ID must be greater than 0"));
                _logger.LogInformation("Retrieving showtime details for ID: {ShowtimeId}", id);
                var showtime = await _showtimeService.GetShowtimeDetailsAsync(id);
                if (showtime == null)
                {
                    _logger.LogWarning("Showtime with ID {ShowtimeId} not found", id);
                    return NotFound(new ApiResponse($"Showtime with ID {id} not found"));
                }
                return Ok(showtime);

            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid input: {Message}", ex.Message);
                return BadRequest(new ApiResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving showtime details");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse("An error occurred while retrieving showtime details"));
            }
        }

        // Get showtimes for a specific movie
        [HttpGet("movie/{movieId}")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResultDTO<ShowtimeDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedResultDTO<ShowtimeDTO>>> GetShowtimesByMovie(int movieId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (movieId <= 0)
                    return BadRequest(new ApiResponse("Movie ID must be greater than 0"));
                _logger.LogInformation("Retrieving showtimes for movie {MovieId}", movieId);
                var result = await _showtimeService.GetShowtimesByMovieAsync(movieId, pageNumber, pageSize);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid parameters: {Message}", ex.Message);
                return BadRequest(new ApiResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Movie not found: {Message}", ex.Message);
                return NotFound(new ApiResponse(ex.Message));
            }
        }
        // Get showtimes for a movie on a specific date
        [HttpGet("movie/{movieId}/date")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ShowtimeDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<ShowtimeDTO>>>> GetShowtimesByMovieAndDate(int movieId, [FromQuery] DateTime date)
        {
            try
            {
                if (movieId <= 0)
                    return BadRequest(new ApiResponse("Movie ID must be greater than 0"));
                _logger.LogInformation("Retrieving showtimes for movie {MovieId}", movieId);
                var result = await _showtimeService.GetShowtimesByMovieAndDateAsync(movieId, date);
                return Ok(new ApiResponse<IEnumerable<ShowtimeDTO>>(
                    result,
                    $"Retrieved {result.Count()} showtimes for specified date"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid parameters: {Message}", ex.Message);
                return BadRequest(new ApiResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving showtimes by date");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse("An error occurred"));
            }
        }
        // Get showtimes in a specific theater
        [HttpGet("theater/{theaterId}")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResultDTO<ShowtimeDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PaginatedResultDTO<ShowtimeDTO>>>> GetShowtimesByTheater
            (int theaterId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (theaterId <= 0)
                    return BadRequest(new ApiResponse("Theater ID must be greater than 0"));
                var result = await _showtimeService.GetShowtimesByTheaterAsync(theaterId, pageNumber, pageSize);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new ApiResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving showtimes by theater");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse("An error occurred"));
            }
        }
        // Get showtime availability status
        [HttpGet("{showtimeId}/availability")]
        [ProducesResponseType(typeof(ApiResponse<ShowtimeAvailabilityDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ShowtimeAvailabilityDTO>>> GetAvailability(int showtimeId)
        {
            try
            {
                var availability = await _showtimeService.GetShowtimeAvailabilityAsync(showtimeId);
                if (availability == null)
                    return NotFound(new ApiResponse("Showtime not found"));
                return Ok(availability);
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error checking availability");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse("An error occurred"));
            }
        }
        // Get reserved seats for a showtime 
        [HttpGet("{showtimeId}/reserved-seats")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<int>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<IEnumerable<int>>>> GetReservedSeats(int showtimeId)
        {
            try
            {
                var reservedSeats = await _showtimeService.GetReservedSeatsAsync(showtimeId);
                return Ok(new ApiResponse<IEnumerable<int>>(
                     reservedSeats,
                     "Reserved seats retrieved successfully"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reserved seats");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse("An error occurred"));
            }
        }
        // Create a new showtime (Admin only)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<ShowtimeDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ShowtimeDTO>>> CreateShowtime([FromBody] CreateShowtimeDTO createshowtimeDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ApiResponse("Invalid showtime data"));
                _logger.LogInformation("Creating new showtime");
                var result = await _showtimeService.CreateShowtimeAsync(createshowtimeDTO);
                return CreatedAtAction(nameof(GetShowtimeDetails),
                     new { id = result.Id },
                     new ApiResponse<ShowtimeDTO>(result, "Showtime created successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid data: {Message}", ex.Message);
                return BadRequest(new ApiResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation: {Message}", ex.Message);
                return BadRequest(new ApiResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating showtime");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse("An error occurred while creating showtime"));
            }
        }
        // Update a showtime (Admin only)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateShowtime([FromBody] CreateShowtimeDTO createshowtimeDTO, int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new ApiResponse("Showtime ID must be greater than 0"));
                if (!ModelState.IsValid)
                    return BadRequest(new ApiResponse("Invalid showtime data"));
                _logger.LogInformation("Updating showtime {ShowtimeId}", id);
                var updated = await _showtimeService.UpdateShowtimeAsync(id, createshowtimeDTO);
                if (!updated)
                    return NotFound(new ApiResponse($"Showtime with ID {id} not found"));
                return Ok(updated);

            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating showtime");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse("An error occurred"));
            }
        }
        // Delete a showtime (Admin only)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteShowtime(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new ApiResponse("Showtime ID must be greater than 0"));
                _logger.LogInformation("Deleting showtime {ShowtimeId}", id);
                var deleted = await _showtimeService.DeleteShowtimeAsync(id);
                if (!deleted) return NotFound(new ApiResponse($"Showtime with ID {id} not found"));
                return Ok(deleted);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Cannot delete: {Message}", ex.Message);
                return BadRequest(new ApiResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting showtime");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse("An error occurred"));
            }
        }
        // Get upcoming showtimes
        [HttpGet("upcoming")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ShowtimeDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<IEnumerable<ShowtimeDTO>>>> GetUpcomingShowtimes(
            [FromQuery] int daysAhead = 7)
        {
            try
            {
                var show = await _showtimeService.GetUpcomingShowtimesAsync(daysAhead);
                return Ok(show);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving upcoming showtimes");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse("An error occurred"));
            }
        }
    }
}
