using System;
using System.Collections.Generic;
using System.Text;

namespace Dashboard.Services.Exceptions
{
    public class AuthException : Exception
    {
        public AuthException(string msg) : base(msg) { }
    }
}