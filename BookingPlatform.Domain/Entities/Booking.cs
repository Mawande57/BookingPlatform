using BookingPlatform.Domain.enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookingPlatform.Domain.Entities
{
    public class Booking
    {
        public Guid Id { get; set; }

        public Guid CustomerId { get; set; }
        public User Customer { get; set; } = null!;

        public Guid SlotId { get; set; }
        public Slot Slot { get; set; } = null!;

        public Guid ProviderServiceId { get; set; }
        public ProviderService ProviderService { get; set; } = null!;

        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        public decimal PriceAtBooking { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
