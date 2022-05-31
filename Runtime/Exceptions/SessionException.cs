using System;

namespace SturfeeVPS.Core
{
    public class SessionException : IdException
    {
        public SessionException((string, string) pair) : base(pair)
        {
        }

        public SessionException(string id, string message) : base(id, message)
        {
        }

        public SessionException(IdException e) : base(e.IdError)
        {

        }
    }
}