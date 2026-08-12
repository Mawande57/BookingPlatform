using System;
using System.Collections.Generic;
using System.Text;

namespace BookingPlatform.Domain.Entities
{
    public enum UserRole
    {
        Customer,
        Provider
    }

    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
