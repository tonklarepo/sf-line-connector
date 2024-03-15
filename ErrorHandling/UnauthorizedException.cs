using System;
namespace ErrorHandling
{
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException()
        {

        }
        public UnauthorizedException(string msg) : base(msg)
        {

        }
    }
}