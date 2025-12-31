using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieReservation.Data.Entities;

namespace MovieReservation.Infrastructure.Data.Configurations
{
    /// <summary>
    /// EF Core configuration for Payment entity
    /// </summary>
    public class PaymentConfigurations : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(p => p.Currency)
                .HasMaxLength(3)
                .IsRequired()
                .HasDefaultValue("USD");

            builder.Property(p => p.Status)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(p => p.StripePaymentIntentId)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(p => p.AppUserId)
                .IsRequired();

            builder.Property(p => p.FailureReason)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(p => p.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(p => p.PaidAt)
                .IsRequired(false);

            builder.Property(p => p.RefundedAt)
                .IsRequired(false);

            // Foreign key relationship
            builder.HasOne(p => p.Reservation)
                .WithMany()
                .HasForeignKey(p => p.ReservationId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // Indexes for performance
            builder.HasIndex(p => p.ReservationId)
                .HasDatabaseName("IX_Payment_ReservationId");

            builder.HasIndex(p => p.AppUserId)
                .HasDatabaseName("IX_Payment_AppUserId");

            builder.HasIndex(p => p.Status)
                .HasDatabaseName("IX_Payment_Status");

            builder.HasIndex(p => p.CreatedAt)
                .HasDatabaseName("IX_Payment_CreatedAt");

            // Composite index for user and reservation queries
            builder.HasIndex(p => new { p.AppUserId, p.CreatedAt })
                .HasDatabaseName("IX_Payment_UserId_CreatedAt");

            builder.ToTable("Payments");
        }
    }
}