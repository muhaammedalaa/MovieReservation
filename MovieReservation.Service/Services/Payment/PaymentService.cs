using AutoMapper;
using Microsoft.Extensions.Logging;
using MovieReservation.Data.Contracts;
using MovieReservation.Data.Dtos;
using MovieReservation.Data.Dtos.Payment;
using MovieReservation.Data.Entities;
using MovieReservation.Data.Service.Contract;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Service.Services.Payment
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PaymentService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<PaymentIntentResponseDTO> CreatePaymentIntentAsync(string userId, CreatePaymentIntentDTO createPaymentIntentDTO)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID is required", nameof(userId));

            if (createPaymentIntentDTO.ReservationId <= 0)
                throw new ArgumentException("Reservation ID must be greater than 0", nameof(createPaymentIntentDTO.ReservationId));
            // Get reservation with showtime
            var reservation = await _unitOfWork.Repository<Data.Entities.Reservation>().GetByIdAsync(createPaymentIntentDTO.ReservationId);
            if (reservation == null)
                throw new InvalidOperationException($"Reservation with ID {createPaymentIntentDTO.ReservationId} not found");
            if (reservation.AppUserId != userId)
                throw new UnauthorizedAccessException("User is not authorized for this reservation");
            var showtime = await _unitOfWork.Repository<Data.Entities.Showtime>().GetByIdAsync(reservation.ShowTimeId.Value);
            if (showtime == null)
                throw new InvalidOperationException("Showtime not found for reservation");
            var amount = (long)(showtime.Price * 100);
            var paymentIntentService = new PaymentIntentService();
            try
            {
                var paymentIntentOptions = new PaymentIntentCreateOptions
                {
                    Amount = amount,
                    Currency = "usd",
                    PaymentMethodTypes = new List<string> { "card" },
                    Metadata = new Dictionary<string, string>
                    {
                         { "ReservationId", createPaymentIntentDTO.ReservationId.ToString() },
                         { "UserId", userId }

                    },
                    Description = $"Movie ticket for reservation {createPaymentIntentDTO.ReservationId}"


                };
                var stripePaymentIntent = await paymentIntentService.CreateAsync(paymentIntentOptions);
                // Store payment in database
                var payment = new Data.Entities.Payment
                {
                    ReservationId = createPaymentIntentDTO.ReservationId,
                    AppUserId = userId,
                    Amount = showtime.Price,
                    Currency = "USD",
                    StripePaymentIntentId = stripePaymentIntent.Id,
                    Status = stripePaymentIntent.Status,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Repository<Data.Entities.Payment>().AddAsync(payment);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation(
                   "Payment intent created: {PaymentId}, Reservation: {ReservationId}, Amount: {Amount}",
                   payment.Id, createPaymentIntentDTO.ReservationId, showtime.Price);
                return new PaymentIntentResponseDTO
                {
                    PaymentId = payment.Id,
                    ClientSecret = stripePaymentIntent.ClientSecret,
                    Amount = showtime.Price,
                    Currency = "USD",
                    Status = stripePaymentIntent.Status,
                    StripePaymentIntentId = stripePaymentIntent.Id
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating payment intent");
                throw new InvalidOperationException($"Payment processing failed: {ex.Message}", ex);
            }
        }



        public async Task<PaymentStatusDTO> GetPaymentAsync(string userId, int paymentId)
        {
            var payment = await _unitOfWork.Repository<Data.Entities.Payment>().GetByIdAsync(paymentId);
            if (payment == null || payment.AppUserId != userId)
                return null;
            return new PaymentStatusDTO
            {
                PaymentId = payment.Id,
                ReservationId = payment.ReservationId,
                Status = payment.Status,
                Amount = payment.Amount,
                IsPaid = payment.Status == "succeeded"
            };
        }

        public async Task<bool> IsPaymentCompletedAsync(int reservationId)
        {
            var payments = await _unitOfWork.Repository<Data.Entities.Payment>().GetAllAsync();
            var payment = ((List<Data.Entities.Payment>)payments)
                .Find(p => p.ReservationId == reservationId);
            return payment != null && payment.Status == "succeeded";
        }

        public async Task<PaymentStatusDTO> VerifyPaymentAsync(string userId, int paymentId)
        {
            if (paymentId <= 0)
                throw new ArgumentException("Payment ID must be greater than 0", nameof(paymentId));

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID is required", nameof(userId));
            var payment = await _unitOfWork.Repository<Data.Entities.Payment>().GetByIdAsync(paymentId);
            if (payment == null)
                throw new InvalidOperationException($"Payment with ID {paymentId} not found");

            if (payment.AppUserId != userId)
                throw new UnauthorizedAccessException("User is not authorized for this payment");
            var paymentIntentService = new PaymentIntentService();
            try
            {
                var stripePaymentIntent = await paymentIntentService.GetAsync(payment.StripePaymentIntentId);
                // Update payment status in database
                payment.Status = stripePaymentIntent.Status;
                if (stripePaymentIntent.Status == "succeeded" && payment.PaidAt == null)
                {
                    payment.PaidAt = DateTime.UtcNow;
                    _logger.LogInformation("Payment {PaymentId} confirmed as paid", paymentId);
                }
                _unitOfWork.Repository<Data.Entities.Payment>().Update(payment);
                await _unitOfWork.SaveChangesAsync();
                return new PaymentStatusDTO
                {
                    PaymentId = paymentId,
                    ReservationId = payment.ReservationId,
                    Status = payment.Status,
                    Amount = payment.Amount,
                    IsPaid = payment.Status == "succeeded"
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error verifying payment");
                throw new InvalidOperationException($"Payment verification failed: {ex.Message}", ex);
            }
        }
    }
}
