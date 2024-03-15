namespace Dashboard.API.Xss
{
    public class AntiXssMiddlewareOptions
    {
        public bool ThrowExceptionIfRequestContainsCrossSiteScripting { get; set; }
        public string ErrorMessage { get; set; }
    }
}