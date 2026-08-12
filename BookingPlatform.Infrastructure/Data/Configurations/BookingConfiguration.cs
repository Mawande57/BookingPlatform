using BookingPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookingPlatform.Infrastructure.Data.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.HasIndex(b => b.SlotId).IsUnique();

            builder.Property(b => b.PriceAtBooking)
                .HasPrecision(10, 2);

            builder.Property(b => b.Status)
                .HasConversion<string>();

            builder.HasOne(b => b.Customer)
                .WithMany()
                .HasForeignKey(b => b.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.Slot)
                .WithMany()
                .HasForeignKey(b => b.SlotId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.ProviderService)
                .WithMany()
                .HasForeignKey(b => b.ProviderServiceId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
