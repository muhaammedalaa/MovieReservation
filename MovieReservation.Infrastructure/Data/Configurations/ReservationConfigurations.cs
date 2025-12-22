using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieReservation.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Infrastructure.Data.Configurations
{
    public class ReservationConfigurations : IEntityTypeConfiguration<Reservation>
    {
        public void Configure(EntityTypeBuilder<Reservation> builder)
        {
            builder.Property(r => r.SeatNumber)
                .IsRequired()
                .HasMaxLength(10);
            builder.HasOne(r => r.Showtime)
                .WithMany(s => s.Reservations)
                .HasForeignKey(r => r.ShowTimeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
