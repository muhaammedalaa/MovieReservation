using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Data.Dtos
{
    public class PaymentIntentResponseDTO
    {
        public int PaymentId { get; set; }
        public string ClientSecret { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public string StripePaymentIntentId { get; set; }
        public decimal Amount { get; set; }

    }
}
