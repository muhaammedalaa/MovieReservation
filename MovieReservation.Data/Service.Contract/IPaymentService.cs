using MovieReservation.Data.Dtos;
using MovieReservation.Data.Dtos.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Data.Service.Contract
{
    public interface IPaymentService
    {
        public Task<PaymentIntentResponseDTO> CreatePaymentIntentAsync(string userId, CreatePaymentIntentDTO createPaymentIntentDTO);
        public Task<PaymentStatusDTO> VerifyPaymentAsync(string userId, int paymentId);
        public Task<PaymentStatusDTO> GetPaymentAsync(string userId, int paymentId);
        Task<bool> IsPaymentCompletedAsync(int reservationId);

    }
}
