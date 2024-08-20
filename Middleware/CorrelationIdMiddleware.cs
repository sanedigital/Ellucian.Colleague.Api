// Copyright 2024 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Api.Middleware
{
    /// <summary>
    /// Sets TraceIdentifier to a Guid for Correlation ID usage
    /// </summary>
    public class CorrelationIdMiddleware
    {
        private RequestDelegate _next;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("correlation-id", out StringValues correlationIdValue))
            {
                context.TraceIdentifier = correlationIdValue;
            }
            else
            {
                context.TraceIdentifier = Guid.NewGuid().ToString();
            }
            await _next.Invoke(context);
        }
    }
}
