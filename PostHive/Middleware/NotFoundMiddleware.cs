namespace PostHive.Middleware
{
    // Middleware to handle 404 Not Found responses and redirect to a custom 404 page.
    public class NotFoundMiddleware
    {
        private readonly RequestDelegate _next;

        // Constructor to initialize the middleware with the next delegate in the pipeline.
        public NotFoundMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // Middleware logic to intercept the HTTP context and handle 404 responses.
        public async Task Invoke(HttpContext httpContext)
        {
            // Call the next middleware in the pipeline.
            await _next(httpContext);

            // Check if the response status code is 404 (Not Found).
            if (httpContext.Response.StatusCode == 404)
            {
                // Redirect the user to the custom 404 page.
                httpContext.Response.Redirect("/404");
            }
        }
    }

    // Extension method to simplify the registration of the NotFoundMiddleware in the HTTP request pipeline.
    public static class NotFoundMiddlewareExtensions
    {
        // Adds the NotFoundMiddleware to the application's middleware pipeline.
        public static IApplicationBuilder UseNotFoundMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<NotFoundMiddleware>();
        }
    }
}
