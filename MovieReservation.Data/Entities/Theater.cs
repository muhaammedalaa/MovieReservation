using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Data.Entities
{
    public class Theater
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int totalSeats { get; set; }
        public ICollection<Showtime>? Showtimes { get; set; } 
    }
}
