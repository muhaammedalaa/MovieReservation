using System;

namespace MovieReservation.Data.Entities
{
    /// <summary>
    /// Payment entity for Stripe integration
    /// </summary>
    public class Payment
    {
        public int Id { get; set; }
        public int ReservationId { get; set; }
        public string AppUserId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";

        // Stripe specific
        public string StripePaymentIntentId { get; set; }
        public string Status { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }

        public string? FailureReason { get; set; }
        public DateTime? RefundedAt { get; set; }
        // Navigation
        public Reservation? Reservation { get; set; }
    }
}