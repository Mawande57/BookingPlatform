using BookingPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookingPlatform.Infrastructure.Data.Configurations
{
    public class AvailabilityTemplateConfiguration : IEntityTypeConfiguration<AvailabilityTemplate>
    {
        public void Configure(EntityTypeBuilder<AvailabilityTemplate> builder)
        {
            builder.HasOne(t => t.Provider)
                .WithMany()
                .HasForeignKey(t => t.ProviderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
