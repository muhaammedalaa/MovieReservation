namespace MovieReservation.Data.Dtos.Payment
{
    /// <summary>
    /// DTO for payment status
    /// </summary>
    public class PaymentStatusDTO
    {
        public int PaymentId { get; set; }
        public int ReservationId { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public bool IsPaid { get; set; }
    }
}