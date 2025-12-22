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
    public class ShowTimesConfigurations : IEntityTypeConfiguration<Showtime>
    {
        public void Configure(EntityTypeBuilder<Showtime> builder)
        {
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Price)
                .IsRequired()
                .HasColumnType("decimal(18,2)");
            builder.Property(s => s.StartDate)
                .IsRequired();
            builder.HasMany(s => s.Reservations)
                .WithOne(r => r.Showtime)
                .HasForeignKey(r => r.ShowTimeId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(s => s.Movie)
                .WithMany(m => m.Showtimes)
                .HasForeignKey(s => s.MovieId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(s => s.Theater)
                .WithMany(t => t.Showtimes)
                .HasForeignKey(s => s.TheaterId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
