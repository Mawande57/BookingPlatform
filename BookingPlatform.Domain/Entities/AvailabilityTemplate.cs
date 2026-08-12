using System;
using System.Collections.Generic;
using System.Text;

namespace BookingPlatform.Domain.Entities
{
    public class AvailabilityTemplate
    {
        public Guid Id { get; set; }

        public Guid ProviderId { get; set; }
        public Provider Provider { get; set; } = null!;

        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int SlotDurationMinutes { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
