using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MovieReservation.Data.Dtos;
using MovieReservation.Data.Service.Contract;
using System.Security.Claims;

namespace MovieReservation.APi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        private readonly IEmailService _emailService;

        public AuthController(IAuthService authService, ILogger<AuthController> logger,IEmailService emailService)
        {
            _authService = authService;
            _logger = logger;
            _emailService = emailService;
        }
        // Register a new user account
        [HttpPost("Register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<AuthResponseDTO>>> Register([FromBody] RegisterDTO registerDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ApiResponse("Invalid registration data"));
                _logger.LogInformation("Registration attempt for email: {Email}", registerDTO.Email);
                var result = await _authService.RegisterAsync(registerDTO);
                if (!result.IsAuthenticated)
                    return BadRequest(new ApiResponse(result.Message));
                try
                {
                    await _emailService.SendRegistrationConfirmationAsync(
                        registerDTO.Email,
                        registerDTO.Name);

                    _logger.LogInformation("Registration confirmation email sent to: {Email}", registerDTO.Email);
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, "Failed to send registration email to: {Email}", registerDTO.Email);
                    // Don't fail registration if email fails - user can still login
                }
                return Ok(new ApiResponse<AuthResponseDTO>(result, "Registration successful. Please login."));


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse("An error occurred during registration"));
            }
        }
        // Login user and receive JWT token
        [HttpPost("Login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<AuthResponseDTO>>> Login([FromBody] LoginDTO loginDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ApiResponse("Invalid login credentials"));

                _logger.LogInformation("Login attempt for email: {Email}", loginDTO.Email);
                var result = await _authService.LoginAsync(loginDTO);
                if (!result.IsAuthenticated)
                    return Unauthorized(new ApiResponse(result.Message));
                return Ok(new ApiResponse<AuthResponseDTO>(result, "Login successful"));
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error during login");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse("An error occurred during login"));
            }

        }
        [HttpPost("Logout")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> Logout()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponse("User not authenticated"));
                _logger.LogInformation("Logout attempt for user: {UserId}", userId);
                await _authService.LogoutAsync(userId);
                return Ok();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse("An error occurred during logout"));
            }

        }
        // Change user password
        [HttpPost("ChangePassword")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> ChangePassword([FromBody] ChangePasswordDTO changePasswordDTO)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(userId))
                    return Unauthorized(new ApiResponse("User not authenticated"));
                if (!ModelState.IsValid)
                    return BadRequest(new ApiResponse("User not authenticated"));
                _logger.LogInformation("Password change attempt for user: {UserId}", userId);
                var result = await _authService.ChangePasswordAsync(userId, changePasswordDTO);
                if (!result)
                    return BadRequest(new ApiResponse("Password change failed. Check your current password."));
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse("An error occurred while changing password"));
            }
        }
        // Get current user profile
        [HttpGet("Profile")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<dynamic>>> GetProfile()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(userId))
                    return Unauthorized(new ApiResponse("User not authenticated"));
                var profile = await _authService.GetUserProfileAsync(userId);
                if (profile == null)
                    return Unauthorized(new ApiResponse("User not found"));
                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving profile");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse("An error occurred while retrieving profile"));
            }
        }
    }
}
