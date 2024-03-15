using System;

namespace PagingExtensions
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class PagingResponseHeadersAttribute : Attribute
    {
    }
}