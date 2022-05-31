using System;

namespace SturfeeVPS.Core
{
    public class IdException : Exception
    {
        public IdException(string id, string message) : base(message)
        {
            Id = id;
            IdError = (id, message);
        }

        public IdException((string, string) pair) : base(pair.Item2)
        {
            Id = pair.Item1;
            IdError = pair;
        }

        public string Id { private set; get; }

        public (string, string) IdError { private set; get; }
    }
}