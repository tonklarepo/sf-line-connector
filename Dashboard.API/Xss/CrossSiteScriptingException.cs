using System;
using System.Runtime.Serialization;

namespace Dashboard.API.Xss
{
    [Serializable]
    internal class CrossSiteScriptingException : Exception
    {
        private object errorMessage;

        public CrossSiteScriptingException()
        {
        }

        public CrossSiteScriptingException(object errorMessage)
        {
            this.errorMessage = errorMessage;
        }

        public CrossSiteScriptingException(string message) : base(message)
        {
        }

        public CrossSiteScriptingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CrossSiteScriptingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}