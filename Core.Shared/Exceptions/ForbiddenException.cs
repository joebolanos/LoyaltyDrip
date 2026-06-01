using System;

namespace Core.Shared.Exceptions
{
    public class ForbiddenException : Exception
    {
        public ForbiddenException() : base() { }
        public ForbiddenException(string message) : base(message) { }
    }
}
