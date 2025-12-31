using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MovieReservation.Data.Contracts;
using MovieReservation.Data.Dtos;
using MovieReservation.Data.Dtos.Payment;
using MovieReservation.Data.Entities;
using MovieReservation.Data.Entities.Identity;
using MovieReservation.Data.Service.Contract;
using System.Security.Claims;

namespace MovieReservation.APi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;

        public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger, IEmailService emailService, IUnitOfWork unitOfWork, UserManager<AppUser> userManager)
        {
            _paymentService = paymentService;
            _logger = logger;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        // Create a Stripe payment intent for a reservation
        [HttpPost("CreatePaymentIntent")]
        [ProducesResponseType(typeof(ApiResponse<PaymentIntentResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PaymentIntentResponseDTO>>> CreatePaymentIntent([FromBody] CreatePaymentIntentDTO createPayment)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(userId))
                    return Unauthorized(new ApiResponse("User is not authenticated"));

                if (!ModelState.IsValid)
                    return BadRequest(new ApiResponse("Invalid payment data"));

                _logger.LogInformation("Creating payment intent for reservation {ReservationId}", createPayment.ReservationId);
                var result = await _paymentService.CreatePaymentIntentAsync(userId, createPayment);
                return Ok(new ApiResponse<PaymentIntentResponseDTO>(
                   result,
                   "Payment intent created. Use clientSecret with Stripe.js on frontend."));

            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid request: {Message}", ex.Message);
                return BadRequest(new ApiResponse(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized: {Message}", ex.Message);
                return Unauthorized(new ApiResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation: {Message}", ex.Message);
                return BadRequest(new ApiResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment intent");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse("An error occurred while creating payment intent"));
            }
        }
        // Verify payment status with Stripe
        [HttpPost("VerifyPayment/{paymentId}")]
        [ProducesResponseType(typeof(ApiResponse<PaymentStatusDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PaymentStatusDTO>>> VerifyPayment(int paymentId)
        {
            try
            {

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(userId))
                    return Unauthorized(new ApiResponse("User is not authenticated"));

                _logger.LogInformation("Verifying payment {PaymentId}", paymentId);
                var result = await _paymentService.VerifyPaymentAsync(userId, paymentId);
                if (!result.IsPaid)
                    return BadRequest(new ApiResponse($"Payment status: {result.Status}"));
                try
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    var reservation = await _unitOfWork.Repository<Reservation>()
                        .GetByIdAsync(result.ReservationId);

                    if (user != null && reservation != null)
                    {
                        await _emailService.SendPaymentSuccessAsync(
                            user.Email,
                            result.ReservationId.ToString(),
                            reservation.SecretCode);

                        _logger.LogInformation("Payment confirmation email sent to: {Email}", user.Email);
                    }
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, "Failed to send payment confirmation email");
                    // Continue anyway - payment is verified
                }
                return Ok(new ApiResponse<PaymentStatusDTO>(
                    result,
                    "Payment verified successfully"));
            }
            catch (ArgumentException ex)
            {

                return BadRequest(new ApiResponse(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new ApiResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying payment");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse("An error occurred while verifying payment"));
            }

        }
        // Get payment status
        [HttpGet("{paymentId}")]
        [ProducesResponseType(typeof(ApiResponse<PaymentStatusDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PaymentStatusDTO>>> GetPaymentStatus(int paymentId)
        {
            try
            {

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(userId))
                    return Unauthorized(new ApiResponse("User is not authenticated"));
                var payment = await _paymentService.GetPaymentAsync(userId, paymentId);
                if (payment == null)
                    return NotFound(new ApiResponse("Payment not found"));
                return Ok(new ApiResponse<PaymentStatusDTO>(payment, "Payment status retrieved"));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment status");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse("An error occurred"));
            }
        }

    }
}
