using System.Net;

namespace Base.DTOs
{
    public class APIResponse_Auth
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }

        public APIResponse_Auth()
        {
            Message = "";
        }
    }
}