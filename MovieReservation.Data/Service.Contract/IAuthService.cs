using MovieReservation.Data.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Data.Service.Contract
{
    public interface IAuthService
    {
        // Register a new user
        Task<AuthResponseDTO> RegisterAsync(RegisterDTO registerDTO);
        // Login user and return JWT token
        Task<AuthResponseDTO> LoginAsync(LoginDTO loginDTO);
        // Logout user
        Task LogoutAsync(string userId);
        // Change user password
        Task<bool> ChangePasswordAsync(string userId, ChangePasswordDTO changePasswordDTO);
        // Refresh JWT token
        Task<AuthResponseDTO> RefreshTokenAsync(string refreshToken);
        // Get current user profile
        Task<dynamic> GetUserProfileAsync(string userId);
    }
}
