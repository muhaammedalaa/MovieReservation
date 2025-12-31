namespace MovieReservation.Data.Service.Contract
{
    public interface IEmailService
    {
        // Send registration confirmation email
        Task SendRegistrationConfirmationAsync(string email, string userName);
        // Send payment success notification
        Task SendPaymentSuccessAsync(string email, string reservationId, string secretCode);

        // Send payment failure notification
        Task SendPaymentFailureAsync(string email, string reason);
        // Send password reset email
        Task SendPasswordResetAsync(string email, string resetLink);
        // Send reservation reminder
        Task SendReservationReminderAsync(string email, string movieTitle, DateTime showTime);
        // Send generic email
        Task SendEmailAsync(string to, string subject, string htmlBody);



    }
}
