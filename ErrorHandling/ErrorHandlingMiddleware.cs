using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ErrorHandling
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _logger = logger;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unhandle Error: {ex}");
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            ValidateException validateException = null;
            UnauthorizedException unauthorizedException = null;
            if (exception.GetType() == typeof(ValidateException))
            {
                validateException = (ValidateException)exception;
            }
            else if (exception.GetType() == typeof(UnauthorizedException))
            {
                unauthorizedException = (UnauthorizedException)exception;
            }

            if (validateException != null)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return context.Response.WriteAsync(validateException.ErrorResponse.ToString());
            }
            else if (unauthorizedException != null)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                var errorResult = new ErrorResponse()
                {
                    PopupErrors = new List<ErrorItem>(),
                    FieldErrors = new List<ErrorItem>()
                };
                errorResult.PopupErrors.Add(new ErrorItem()
                {
                    Code = "UNAUTHORIZED",
                    Message = $"{exception.Message}"
                });
                return context.Response.WriteAsync(errorResult.ToString());
            }
            else
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                var errorResult = new ErrorResponse()
                {
                    PopupErrors = new List<ErrorItem>(),
                    FieldErrors = new List<ErrorItem>()
                };

                errorResult.PopupErrors.Add(new ErrorItem()
                {
                    Code = "ERR0000",
                    Message = $"{exception.Message} : {exception.StackTrace}"
                });

                return context.Response.WriteAsync(errorResult.ToString());
            }

        }
    }
}
