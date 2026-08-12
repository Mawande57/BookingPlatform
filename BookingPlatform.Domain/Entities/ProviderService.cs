using System;
using System.Collections.Generic;
using System.Text;

namespace BookingPlatform.Domain.Entities
{
    public class ProviderService
    {
        public Guid Id { get; set; }

        public Guid ProviderId { get; set; }
        public Provider Provider { get; set; } = null!;

        public Guid ServiceId { get; set; }
        public Service Service { get; set; } = null!;

        public decimal Price { get; set; }
        public int DurationMinutes { get; set; }
        public byte[] RowVersion { get; set; } = null!;
    }
}
