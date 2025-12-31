using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MovieReservation.Data.Dtos;
using MovieReservation.Data.Entities.Identity;
using MovieReservation.Data.Service.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MovieReservation.Service.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IJwtTokenService jwtTokenService,
            ILogger<AuthService> logger
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
        }
        public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordDTO changePasswordDTO)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId is required", nameof(userId));

            if (changePasswordDTO == null)
                throw new ArgumentNullException(nameof(changePasswordDTO));

            if (changePasswordDTO.NewPassword != changePasswordDTO.ConfirmNewPassword)
                throw new ArgumentException("Passwords do not match");
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;
            var result = await _userManager.ChangePasswordAsync(user, changePasswordDTO.CurrentPassword, changePasswordDTO.NewPassword);
            if (result.Succeeded)
            {
                _logger.LogInformation("Password changed for user: {UserId}", userId);
                return true;
            }
            _logger.LogWarning("Password change failed for user: {UserId}", userId);
            return false;
        }

        public async Task<dynamic> GetUserProfileAsync(string userId)
        {

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId is required", nameof(userId));

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return null;

            var roles = await _userManager.GetRolesAsync(user);
            return new
            {
                user.Id,
                user.Email,
                user.Name,
                user.PhoneNumber,
                user.Birthday,
                user.CreatedAt,
                Roles = roles
            };
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginDTO loginDTO)
        {
            if (loginDTO == null)
                throw new ArgumentNullException(nameof(loginDTO));
            var user = await _userManager.FindByEmailAsync(loginDTO.Email);
            if (user == null)
            {
                _logger.LogWarning("Login attempt failed: user not found for email {Email}", loginDTO.Email);
                return new AuthResponseDTO
                {
                    IsAuthenticated = false,
                    Message = "Invalid email or password"
                };
            }
            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDTO.Password, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Login attempt failed for email {Email}", loginDTO.Email);
                return new AuthResponseDTO
                {
                    IsAuthenticated = false,
                    Message = "Invalid email or password"
                };
            }
            var roles = await _userManager.GetRolesAsync(user);
            var (token, expiration) = _jwtTokenService.GenerateToken(user, roles);
            _logger.LogInformation("User logged in successfully: {Email}", loginDTO.Email);
            return new AuthResponseDTO
            {
                IsAuthenticated = true,
                Message = "Login successful",
                Email = user.Email,
                Username = user.UserName,
                Token = token,
                TokenExpiration = expiration
            };
        }

        public async Task LogoutAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out: {UserId}", userId);

        }

        public Task<AuthResponseDTO> RefreshTokenAsync(string refreshToken)
        {
            throw new NotImplementedException();
        }

        public async Task<AuthResponseDTO> RegisterAsync(RegisterDTO registerDTO)
        {
            if (registerDTO == null)
                throw new ArgumentNullException(nameof(registerDTO));
            if (string.IsNullOrWhiteSpace(registerDTO.Email))
                return new AuthResponseDTO
                {
                    IsAuthenticated = false,
                    Message = "Email is required"
                };
            if (string.IsNullOrEmpty(registerDTO.Password))
                return new AuthResponseDTO
                {
                    IsAuthenticated = false,
                    Message = "Passwords do not match"
                };
            var existingUser = await _userManager.FindByEmailAsync(registerDTO.Email);
            if (existingUser != null)
                return new AuthResponseDTO
                {
                    IsAuthenticated = false,
                    Message = "Email already registered"
                };
            var user = new AppUser
            {
                UserName = registerDTO.Email,
                Email = registerDTO.Email,
                Name = registerDTO.Name,
                PhoneNumber = registerDTO.PhoneNumber,
                Birthday = registerDTO.Birthday,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
            };
            var result = await _userManager.CreateAsync(user, registerDTO.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors);
                _logger.LogWarning("User registration failed for email {Email}: {Errors}", registerDTO.Email, errors);
                return new AuthResponseDTO
                {
                    IsAuthenticated = false,
                    Message = $"Registration failed: {errors}"
                };
            }
            await _userManager.AddToRoleAsync(user, "User");
            _logger.LogInformation("User registered successfully: {Email}", registerDTO.Email);
            return new AuthResponseDTO
            {
                IsAuthenticated = true,
                Message = "Registration successful. Please login.",
                Email = user.Email,
                Username = user.UserName
            };


        }
    }
}
