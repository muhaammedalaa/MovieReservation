using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MovieReservation.Data.Contracts;
using MovieReservation.Data.Entities;
using MovieReservation.Data.Entities.Identity;
using MovieReservation.Data.Service.Contract;
using Stripe;


namespace MovieReservation.APi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<WebhookController> _logger;
        private readonly IEmailService _emailService;
        private readonly UserManager<AppUser> _userManager;
        private readonly string? _stripeWebhookSecret;


        public WebhookController(IUnitOfWork unitOfWork, ILogger<WebhookController> logger, IConfiguration configuration, IEmailService emailService, UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _emailService = emailService;
            _userManager = userManager;
            _stripeWebhookSecret = configuration["Stripe:WebhookSecret"];
        }
        // Receive Stripe webhook events
        [HttpPost("stripe")]
        [IgnoreAntiforgeryToken] // Stripe webhooks don't include CSRF tokens
        public async Task<IActionResult> HandleStripeWebhook()
        {
            try
            {
                // Read raw request body
                var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                // Get Stripe signature from headers
                var stripeSignature = Request.Headers["Stripe-Signature"].ToString();
                if (string.IsNullOrEmpty(stripeSignature))
                {
                    _logger.LogWarning("Stripe webhook received without signature");
                    return BadRequest("Missing Stripe-Signature header");
                }
                // Verify the webhook comes from Stripe
                Event stripeEvent;
                try
                {
                    stripeEvent = EventUtility.ConstructEvent(
                        json,
                        stripeSignature,
                        _stripeWebhookSecret
                        );
                }
                catch (StripeException ex)
                {
                    _logger.LogWarning("Webhook signature verification failed: {Message}", ex.Message);
                    return BadRequest("Invalid webhook signature");
                }
                // Handle different event types
                switch (stripeEvent.Type)
                {
                    case "payment_intent.succeeded":
                        await HandlePaymentIntentSucceededAsync(stripeEvent);
                        break;
                    case "payment_intent.payment_failed":
                        await HandlePaymentIntentFailedAsync(stripeEvent);
                        break;
                    case "charge.refunded":
                        await HandleChargeRefundedAsync(stripeEvent);
                        break;
                    case "payment_intent.canceled":
                        await HandlePaymentIntentCanceledAsync(stripeEvent);
                        break;
                    default:
                        _logger.LogInformation("Unhandled webhook event type: {EventType}", stripeEvent.Type);
                        break;
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling Stripe webhook");
                return StatusCode(500, "Webhook processing failed");
            }
        }
        private async Task HandlePaymentIntentSucceededAsync(Event stripeEvent)
        {
            try
            {
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                if (paymentIntent == null)
                {
                    _logger.LogWarning("PaymentIntent object is null in succeeded event");
                    return;
                }

                _logger.LogInformation("Payment succeeded: {PaymentIntentId}", paymentIntent.Id);

                // Find payment in database by Stripe ID
                var payments = await _unitOfWork.Repository<Payment>().GetAllAsync();
                var payment = ((List<Payment>)payments).FirstOrDefault(p =>
                    p.StripePaymentIntentId == paymentIntent.Id);

                if (payment == null)
                {
                    _logger.LogWarning("Payment not found for Stripe ID: {PaymentIntentId}", paymentIntent.Id);
                    return;
                }

                // Update payment status
                payment.Status = "succeeded";
                payment.PaidAt = DateTime.UtcNow;

                _unitOfWork.Repository<Payment>().Update(payment);
                // Update reservation as paid
                var reservation = await _unitOfWork.Repository<Reservation>().GetByIdAsync(payment.Id);
                if (reservation != null)
                {
                    reservation.IsPaid = true;
                    _unitOfWork.Repository<Reservation>().Update(reservation);

                }
                await _unitOfWork.SaveChangesAsync();
                try
                {
                    var user = await _userManager.FindByIdAsync(payment.AppUserId);
                    if (user != null && reservation != null)
                    {
                        await _emailService.SendPaymentSuccessAsync(
                            user.Email,
                            payment.ReservationId.ToString(),
                            reservation.SecretCode
                            );
                    }
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, "Failed to send payment email for payment: {PaymentId}", payment.Id);
                }
                _logger.LogInformation("Payment {PaymentId} marked as succeeded", payment.Id);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling payment succeeded event");
                throw;
            }
        }
        private async Task HandlePaymentIntentFailedAsync(Event stripeEvent)
        {
            try
            {
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                if (paymentIntent == null)
                {
                    _logger.LogWarning("PaymentIntent object is null in failed event");
                    return;
                }

                _logger.LogInformation("Payment failed: {PaymentIntentId}", paymentIntent.Id);

                // Find payment in database
                var payments = await _unitOfWork.Repository<Payment>().GetAllAsync();
                var payment = ((List<Payment>)payments).FirstOrDefault(p =>
                    p.StripePaymentIntentId == paymentIntent.Id);

                if (payment == null)
                {
                    _logger.LogWarning("Payment not found for Stripe ID: {PaymentIntentId}", paymentIntent.Id);
                    return;
                }

                // Update payment status with failure reason
                payment.Status = "failed";
                payment.FailureReason = paymentIntent.LastPaymentError?.Message ?? "Unknown error";

                _unitOfWork.Repository<Payment>().Update(payment);
                await _unitOfWork.SaveChangesAsync();
                try
                {
                    var user = await _userManager.FindByIdAsync(payment.AppUserId);
                    if (user != null)
                    {
                        await _emailService.SendPaymentFailureAsync(
                            user.Email,
                            payment.FailureReason);

                        _logger.LogInformation("Payment failure email sent to: {Email}", user.Email);
                    }
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, "Failed to send failure email for payment: {PaymentId}", payment.Id);
                }
                _logger.LogWarning("Payment {PaymentId} marked as failed: {Reason}",
                    payment.Id, payment.FailureReason);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling charge refunded event");
                throw;
            }
        }
        private async Task HandleChargeRefundedAsync(Event stripeEvent)
        {
            try
            {
                var charge = stripeEvent.Data.Object as Charge;
                if (charge == null)
                {
                    _logger.LogWarning("Charge object is null in refunded event");
                    return;
                }

                _logger.LogInformation("Charge refunded: {ChargeId}", charge.Id);

                // Find payment by Stripe charge ID
                var payments = await _unitOfWork.Repository<Payment>().GetAllAsync();
                var payment = ((List<Payment>)payments).FirstOrDefault(p =>
                    p.StripePaymentIntentId == charge.PaymentIntentId);

                if (payment == null)
                {
                    _logger.LogWarning("Payment not found for charge: {ChargeId}", charge.Id);
                    return;
                }

                // Update payment status
                payment.Status = "refunded";
                payment.RefundedAt = DateTime.UtcNow;

                _unitOfWork.Repository<Payment>().Update(payment);

                // Update reservation - user keeps their reservation but money is refunded
                var reservation = await _unitOfWork.Repository<Reservation>().GetByIdAsync(payment.ReservationId);
                if (reservation != null)
                {
                    reservation.IsPaid = false;
                    _unitOfWork.Repository<Reservation>().Update(reservation);
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Payment {PaymentId} marked as refunded", payment.Id);

                // TODO: Send refund notification email
                // await _emailService.SendRefundNotificationAsync(payment.AppUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling charge refunded event");
                throw;
            }
        }

        private async Task HandlePaymentIntentCanceledAsync(Event stripeEvent)
        {
            try
            {
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                if (paymentIntent == null)
                {
                    _logger.LogWarning("PaymentIntent object is null in canceled event");
                    return;
                }
                _logger.LogInformation("Payment canceled: {PaymentIntentId}", paymentIntent.Id);
                // Find payment in database
                var payments = await _unitOfWork.Repository<Payment>().GetAllAsync();
                var payment = ((List<Payment>)payments).FirstOrDefault(p =>
                p.StripePaymentIntentId == paymentIntent.Id);
                if (payment == null)
                {
                    _logger.LogWarning("Payment not found for Stripe ID: {PaymentIntentId}", paymentIntent.Id);
                    return;
                }
                payment.Status = "canceled";
                _unitOfWork.Repository<Payment>().Update(payment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Payment {PaymentId} marked as canceled", payment.Id);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling payment canceled event");
                throw;
            }
        }

    }
}
