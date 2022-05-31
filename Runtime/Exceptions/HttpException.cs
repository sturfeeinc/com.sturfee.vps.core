using System;

namespace SturfeeVPS.Core
{
    public class HttpException : Exception
    {
        public long ErrorCode;

        public HttpException(long erroCode, string message) : base(message)
        {
            ErrorCode = erroCode;
        }
    }
}