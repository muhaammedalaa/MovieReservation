using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MovieReservation.Data.Dtos;
using MovieReservation.Data.Entities;
using MovieReservation.Data.Service.Contract;
using System.Security.Claims;

namespace MovieReservation.APi.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _reservationService;
        private readonly IShowtimeService _showtimeService;
        private readonly ILogger<ReservationController> _logger;

        public ReservationController(IReservationService reservationService, IShowtimeService showtimeService, ILogger<ReservationController> logger)
        {
            _reservationService = reservationService;
            _showtimeService = showtimeService;
            _logger = logger;
        }
        // Create a new reservation (book a ticket)
        [HttpPost("CreateReservation")]
        [ProducesResponseType(typeof(ApiResponse<ReservationDetailDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ReservationDetailDTO>>> CreateReservation(CreateReservationDTO request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(userId))
                    return Unauthorized(new ApiResponse("User is not authenticated"));
                if (!ModelState.IsValid)
                    return BadRequest(new ApiResponse("Invalid reservation data"));
                _logger.LogInformation("User {UserId} attempting to create reservation for showtime {ShowtimeId}, seat {SeatNumber}",
                   userId, request.ShowtimeId, request.SeatNumber);
                var result = await _reservationService.CreateReservationAsync(request, userId);
                return Ok(result);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning("Null argument: {Message}", ex.Message);
                return BadRequest(new ApiResponse(ex.Message));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid argument: {Message}", ex.Message);
                return BadRequest(new ApiResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation: {Message}", ex.Message);
                return BadRequest(new ApiResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reservation");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse("An error occurred while creating the reservation"));
            }
        }
        // Get user's reservations with pagination
        [HttpGet("MyReservations")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResultDTO<ReservationDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PaginatedResultDTO<ReservationDTO>>>> GetMyReservations(
             [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(userId))
                    return Unauthorized(new ApiResponse("User is not authenticated"));

                _logger.LogInformation("Retrieving reservations for user {UserId}", userId);
                var result = await _reservationService.GetReservationsByUserAsync(userId, pageNumber, pageSize);
                return Ok(result);

            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid pagination: {Message}", ex.Message);
                return BadRequest(new ApiResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user reservations");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse("An error occurred while retrieving reservations"));
            }
        }
        // Get reservation details by ID
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<ReservationDetailDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ReservationDetailDTO>>> GetReservationDetails(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new ApiResponse("Reservation ID must be greater than 0"));
                _logger.LogInformation("Retrieving reservation details for ID: {ReservationId}", id);
                var result = await _reservationService.GetReservationByIdAsync(id);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid ID: {Message}", ex.Message);
                return BadRequest(new ApiResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Reservation not found: {Message}", ex.Message);
                return NotFound(new ApiResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservation details");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse("An error occurred while retrieving reservation"));
            }
        }

        // Verify a reservation using ID and secret code
        [HttpPost("Verify/{reservationId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<ReservationDetailDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ReservationDetailDTO>>> VerifyReservation(int reservationId, [FromQuery] string secretCode)
        {
            try
            {

                if (reservationId <= 0)
                    return BadRequest(new ApiResponse("Reservation ID must be greater than 0"));
                _logger.LogInformation("Verifying reservation {ReservationId}", reservationId);
                if (string.IsNullOrWhiteSpace(secretCode))
                    return BadRequest(new ApiResponse("Secret code is required"));
                var (isvalid, reservation) = await _reservationService.ValidateReservationAsync(reservationId, secretCode);
                if (!isvalid)
                {
                    _logger.LogWarning("Verification failed for reservation {ReservationId}", reservationId);
                    return Unauthorized(new ApiResponse("Invalid reservation ID or secret code"));
                }

                return Ok();

            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid parameters: {Message}", ex.Message);
                return BadRequest(new ApiResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying reservation");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse("An error occurred while verifying reservation"));
            }
        }

        // Check seat availability for a showtime
        [HttpGet("CheckSeatAvailability")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<SeatAvailabilityDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<SeatAvailabilityDTO>>> CheckSeatAvailability([FromQuery] int showtimeId, [FromQuery] int seatNumber)
        {
            try
            {
                if (showtimeId <= 0)
                    return BadRequest(new ApiResponse("Showtime ID must be greater than 0"));

                if (seatNumber <= 0)
                    return BadRequest(new ApiResponse("Seat number must be greater than 0"));

                _logger.LogInformation("Checking seat {SeatNumber} availability for showtime {ShowtimeId}",
                    seatNumber, showtimeId);
                var rseult = await _reservationService.CheckSeatAvailabilityAsync(showtimeId, seatNumber);
                return Ok(rseult);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid parameters: {Message}", ex.Message);
                return BadRequest(new ApiResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking seat availability");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse("An error occurred while checking availability"));
            }
        }
        // Get all reserved seat numbers for a showtime
        [HttpGet("{showtimeId}/ReservedSeats")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<int>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<int>>>> GetReservedSeats(int showtimeId)
        {
            try
            {
                if (showtimeId <= 0)
                    return BadRequest(new ApiResponse("Showtime ID must be greater than 0"));

                _logger.LogInformation("Retrieving reserved seats for showtime {ShowtimeId}", showtimeId);
                var result = await _reservationService.GetBookedSeatsAsync(showtimeId);
                return Ok(result);

            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid showtime ID: {Message}", ex.Message);
                return BadRequest(new ApiResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Showtime not found: {Message}", ex.Message);
                return BadRequest(new ApiResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reserved seats");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse("An error occurred"));
            }
        }
        // Get all available seat numbers for a showtime
        [HttpGet("{showtimeId}/AvailableSeats")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<int>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<int>>>> GetAvailableSeats(int showtimeId)
        {
            try
            {
                if (showtimeId <= 0)
                    return BadRequest(new ApiResponse("Showtime ID must be greater than 0"));

                _logger.LogInformation("Retrieving available seats for showtime {ShowtimeId}", showtimeId);
                var availableSeats = await _showtimeService.GetShowtimeAvailabilityAsync(showtimeId);
                if (availableSeats == null)
                    return BadRequest(new ApiResponse("Showtime not found"));

                return Ok(availableSeats);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid showtime ID: {Message}", ex.Message);
                return BadRequest(new ApiResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available seats");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse("An error occurred"));
            }
        }

        // Update reservation seat number
        [HttpPut("{reservationId}/UpdateSeat")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateReservationSeat(
            int reservationId,
            [FromQuery] int newSeatNumber)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(userId))
                    return Unauthorized(new ApiResponse("User is not authenticated"));

                if (reservationId <= 0)
                    return BadRequest(new ApiResponse("Reservation ID must be greater than 0"));

                if (newSeatNumber <= 0)
                    return BadRequest(new ApiResponse("Seat number must be greater than 0"));

                _logger.LogInformation("User {UserId} attempting to update reservation {ReservationId} seat to {NewSeat}",
                    userId, reservationId, newSeatNumber);
                var updated = await _reservationService.UpdateReservationSeatAsync(reservationId, newSeatNumber, userId);
                if (!updated)
                    return NotFound(new ApiResponse($"Reservation with ID {reservationId} not found"));
                return Ok(updated);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid argument: {Message}", ex.Message);
                return BadRequest(new ApiResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation: {Message}", ex.Message);
                return BadRequest(new ApiResponse(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized: {Message}", ex.Message);
                return Unauthorized(new ApiResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating reservation");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse("An error occurred while updating reservation"));
            }
        }
        // Cancel a reservation
        [HttpDelete("{reservationId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CancelReservation(int reservationId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(userId))
                    return Unauthorized(new ApiResponse("User is not authenticated"));

                if (reservationId <= 0)
                    return BadRequest(new ApiResponse("Reservation ID must be greater than 0"));

                _logger.LogInformation("User {UserId} attempting to cancel reservation {ReservationId}",
                    userId, reservationId);
                var result = await _reservationService.CancelReservationAsync(reservationId, userId);
                if (!result)
                    return NotFound(new ApiResponse($"Reservation with ID {reservationId} not found"));
                return Ok(result);

            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid argument: {Message}", ex.Message);
                return BadRequest(new ApiResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation: {Message}", ex.Message);
                return BadRequest(new ApiResponse(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized: {Message}", ex.Message);
                return Unauthorized(new ApiResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling reservation");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse("An error occurred while cancelling reservation"));
            }
        }
        // Check if a reservation exists
        [HttpHead("{reservationId}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ReservationExists(int reservationId)
        {
            try
            {
                if (reservationId <= 0)
                    return BadRequest(new ApiResponse("Reservation ID must be greater than 0"));
                var exists = await _reservationService.ReservationExistsAsync(reservationId);
                return exists ? Ok() : NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse(ex.Message));
            }
        }
    }
}
