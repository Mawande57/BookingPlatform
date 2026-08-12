using BookingPlatform.Domain.enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookingPlatform.Domain.Entities
{
    public class Slot
    {
        public Guid Id { get; set; }

        public Guid ProviderId { get; set; }
        public Provider Provider { get; set; } = null!;

        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        public SlotStatus Status { get; set; } = SlotStatus.Open;

        public Guid? TemplateId { get; set; }
        public AvailabilityTemplate? Template { get; set; }
    }
}
