using System;
using System.Collections.Generic;
using System.Text;

namespace BookingPlatform.Application.Exceptions
{
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
    }
}
