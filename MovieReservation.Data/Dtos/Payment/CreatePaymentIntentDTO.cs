namespace MovieReservation.Data.Dtos.Payment
{
    /// <summary>
    /// DTO for creating a Stripe payment intent
    /// </summary>
    public class CreatePaymentIntentDTO
    {
        public int ReservationId { get; set; }
    }
}