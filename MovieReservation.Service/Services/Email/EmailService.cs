using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MovieReservation.Data.Service.Contract;
using MovieReservation.Service.Models;

namespace MovieReservation.Service.Services.Email
{
    /// <summary>
    /// Email notification service using SMTP (MailKit)
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly EmailSettings _emailSettings;

        public EmailService(ILogger<EmailService> logger, IOptions<EmailSettings> emailSettings)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _emailSettings = emailSettings?.Value ?? throw new ArgumentNullException(nameof(emailSettings));
        }

        /// <summary>
        /// Send registration confirmation email
        /// </summary>
        public async Task SendRegistrationConfirmationAsync(string email, string userName)
        {
            var subject = "Welcome to Movie Reservation!";
            var htmlBody = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif;'>
                            <h2>Welcome, {userName}!</h2>
                            <p>Thank you for registering with Movie Reservation.</p>
                            <p>Your account has been successfully created.</p>
                            <p>You can now:</p>
                            <ul>
                                <li>Browse and search for movies</li>
                                <li>Reserve movie tickets</li>
                                <li>Make secure payments</li>
                            </ul>
                            <p>Happy watching!</p>
                            <br/>
                            <p style='color: #666; font-size: 12px;'>Movie Reservation Team</p>
                        </body>
                        </html>";

            await SendEmailAsync(email, subject, htmlBody);
        }

        /// <summary>
        /// Send payment success notification
        /// </summary>
        public async Task SendPaymentSuccessAsync(string email, string reservationId, string secretCode)
        {
            var subject = "✓ Payment Confirmed - Your Tickets Are Ready!";
            var htmlBody = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif;'>
                            <h2>Payment Successful!</h2>
                            <p>Your payment has been processed successfully.</p>
                            <div style='background-color: #f0f0f0; padding: 20px; border-radius: 5px; margin: 20px 0;'>
                                <p><strong>Reservation ID:</strong> {reservationId}</p>
                                <p><strong>Secret Code:</strong> <span style='font-size: 18px; font-weight: bold;'>{secretCode}</span></p>
                            </div>
                            <p>Please save your secret code. You'll need it at the theater to validate your ticket.</p>
                            <p>Thank you for your purchase!</p>
                            <br/>
                            <p style='color: #666; font-size: 12px;'>Movie Reservation Team</p>
                        </body>
                        </html>";

            await SendEmailAsync(email, subject, htmlBody);
        }

        /// <summary>
        /// Send payment failure notification
        /// </summary>
        public async Task SendPaymentFailureAsync(string email, string reason)
        {
            var subject = "✗ Payment Failed - Action Required";
            var htmlBody = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif;'>
                            <h2>Payment Failed</h2>
                            <p>Unfortunately, your payment could not be processed.</p>
                            <div style='background-color: #ffe0e0; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                <p><strong>Reason:</strong> {reason}</p>
                            </div>
                            <p>Please try again or contact our support team for assistance.</p>
                            <p><a href='https://moviereservation.com/support' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Contact Support</a></p>
                            <br/>
                            <p style='color: #666; font-size: 12px;'>Movie Reservation Team</p>
                        </body>
                        </html>";

            await SendEmailAsync(email, subject, htmlBody);
        }

        /// <summary>
        /// Send password reset email
        /// </summary>
        public async Task SendPasswordResetAsync(string email, string resetLink)
        {
            var subject = "Reset Your Password";
            var htmlBody = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif;'>
                            <h2>Password Reset Request</h2>
                            <p>We received a request to reset your password.</p>
                            <p>Click the link below to reset your password:</p>
                            <p><a href='{resetLink}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Reset Password</a></p>
                            <p>This link will expire in 24 hours.</p>
                            <p>If you didn't request this, please ignore this email.</p>
                            <br/>
                            <p style='color: #666; font-size: 12px;'>Movie Reservation Team</p>
                        </body>
                        </html>";

            await SendEmailAsync(email, subject, htmlBody);
        }

        /// <summary>
        /// Send reservation reminder
        /// </summary>
        public async Task SendReservationReminderAsync(string email, string movieTitle, DateTime showTime)
        {
            var subject = $"Reminder: {movieTitle} starts in 1 hour!";
            var htmlBody = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif;'>
                            <h2>Movie Reminder</h2>
                            <p>Your movie is starting soon!</p>
                            <div style='background-color: #fff3cd; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                <p><strong>Movie:</strong> {movieTitle}</p>
                                <p><strong>Start Time:</strong> {showTime:MMMM dd, yyyy hh:mm tt}</p>
                            </div>
                            <p>Please arrive 15 minutes early.</p>
                            <p>Don't forget your secret code!</p>
                            <br/>
                            <p style='color: #666; font-size: 12px;'>Movie Reservation Team</p>
                        </body>
                        </html>";

            await SendEmailAsync(email, subject, htmlBody);
        }

        /// <summary>
        /// Generic email sending method
        /// Handles the actual SMTP connection and email delivery
        /// </summary>
        public async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(to))
                throw new ArgumentException("Email recipient cannot be empty", nameof(to));

            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentException("Email subject cannot be empty", nameof(subject));

            if (string.IsNullOrWhiteSpace(htmlBody))
                throw new ArgumentException("Email body cannot be empty", nameof(htmlBody));

            try
            {
                // Use MailKit.Net.Smtp.SmtpClient (explicitly qualified to avoid ambiguity)
                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    // Connect to SMTP server
                    await client.ConnectAsync(
                        _emailSettings.SmtpServer,
                        _emailSettings.SmtpPort,
                        _emailSettings.EnableSSL);

                    _logger.LogDebug("Connected to SMTP server: {SmtpServer}:{SmtpPort}",
                        _emailSettings.SmtpServer, _emailSettings.SmtpPort);

                    // Authenticate with credentials
                    await client.AuthenticateAsync(
                        _emailSettings.SenderEmail,
                        _emailSettings.SenderPassword);

                    _logger.LogDebug("Authenticated with email: {SenderEmail}", _emailSettings.SenderEmail);

                    // Create email message using MimeKit
                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress(
                        _emailSettings.SenderName,
                        _emailSettings.SenderEmail));
                    message.To.Add(MailboxAddress.Parse(to));
                    message.Subject = subject;

                    // Create HTML body
                    var bodyBuilder = new BodyBuilder();
                    bodyBuilder.HtmlBody = htmlBody;
                    message.Body = bodyBuilder.ToMessageBody();

                    // Send email
                    await client.SendAsync(message);

                    _logger.LogInformation("Email sent successfully to: {Recipient} with subject: {Subject}", to, subject);

                    // Disconnect gracefully
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to: {Email}, Subject: {Subject}", to, subject);
                throw;
            }
        }
    }
}
