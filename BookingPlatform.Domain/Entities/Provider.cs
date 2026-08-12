using System;
using System.Collections.Generic;
using System.Text;

namespace BookingPlatform.Domain.Entities
{
    public class Provider
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public string BusinessName { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public bool IsActive { get; set; } = true;

        //for furture use if possible and voiding mulitple muigrations for adding new services to provider
        public ICollection<ProviderService> ProviderServices { get; set; } = new List<ProviderService>();
    }
}
