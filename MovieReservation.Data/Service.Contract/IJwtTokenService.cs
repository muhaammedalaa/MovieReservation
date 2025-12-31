using MovieReservation.Data.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Data.Service.Contract
{
    public interface IJwtTokenService
    {
        public (string Token, DateTime expiration) GenerateToken(AppUser user, IList<string> roles);
        public ClaimsPrincipal? VaidateToken(string token);
    }
}
