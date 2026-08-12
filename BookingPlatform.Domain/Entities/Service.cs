using System;
using System.Collections.Generic;
using System.Text;

namespace BookingPlatform.Domain.Entities
{
    public class Service
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int DefaultDurationMinutes { get; set; }
        //for furture use if possible and voiding mulitple muigrations for adding new services to provider
        public ICollection<ProviderService> ProviderServices { get; set; } = new List<ProviderService>();
    }
}
