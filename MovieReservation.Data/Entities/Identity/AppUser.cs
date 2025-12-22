using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Data.Entities.Identity
{
    public class AppUser : IdentityUser
    {
        public string Name { get; set; }
        public DateOnly Birthday { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public ICollection<Reservation>? Reservations { get; set; }
       
    }
}
