using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Dashboard.API.Xss
{
    public class AntiXssMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly AntiXssMiddlewareOptions _options;

        public AntiXssMiddleware(RequestDelegate next, AntiXssMiddlewareOptions options)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            _next = next;
            _options = options;
        }

        public async Task Invoke(HttpContext context)
        {
            // Check XSS in URL
            if (!string.IsNullOrWhiteSpace(context.Request.Path.Value))
            {
                var url = context.Request.Path.Value;

                int matchIndex;
                if (CrossSiteScriptingValidation.IsDangerousString(url, out matchIndex))
                {
                    if (_options.ThrowExceptionIfRequestContainsCrossSiteScripting)
                    {
                        throw new CrossSiteScriptingException(_options.ErrorMessage);
                    }

                    context.Response.Clear();
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await context.Response.WriteAsync(_options.ErrorMessage);
                    return;
                }
            }

            // Check XSS in query string
            if (!string.IsNullOrWhiteSpace(context.Request.QueryString.Value))
            {
                var queryString = WebUtility.UrlDecode(context.Request.QueryString.Value);

                int matchIndex;
                if (CrossSiteScriptingValidation.IsDangerousString(queryString, out matchIndex))
                {
                    if (_options.ThrowExceptionIfRequestContainsCrossSiteScripting)
                    {
                        throw new CrossSiteScriptingException(_options.ErrorMessage);
                    }

                    context.Response.Clear();
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await context.Response.WriteAsync(_options.ErrorMessage);
                    return;
                }
            }

            // Check XSS in request content
            var originalBody = context.Request.Body;
            try
            {
                var content = await ReadRequestBody(context);

                int matchIndex;
                if (CrossSiteScriptingValidation.IsDangerousString(content, out matchIndex))
                {
//                    if (_options.ThrowExceptionIfRequestContainsCrossSiteScripting)
//                    {
//                        throw new CrossSiteScriptingException(_options.ErrorMessage);
//                    }
//
//                    context.Response.Clear();
//                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
//                    await context.Response.WriteAsync(_options.ErrorMessage);
//                    return;
                }

                await _next(context);
            }
            finally
            {
                context.Request.Body = originalBody;
            }
        }

        private static async Task<string> ReadRequestBody(HttpContext context)
        {
            var buffer = new MemoryStream();
            await context.Request.Body.CopyToAsync(buffer);
            context.Request.Body = buffer;
            buffer.Position = 0;

            var encoding = Encoding.UTF8;
            var contentType = context.Request.GetTypedHeaders().ContentType;
            if (contentType?.Charset != null && !string.IsNullOrEmpty(contentType?.Charset.ToString())) encoding = Encoding.GetEncoding(contentType.Charset.ToString());

            var requestContent = await new StreamReader(buffer, encoding).ReadToEndAsync();
            context.Request.Body.Position = 0;

            return requestContent;
        }
    }
}
