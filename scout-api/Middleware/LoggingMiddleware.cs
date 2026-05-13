using scout_api.Services;

namespace scout_api.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, LoggingService loggingService, SessionService sessionService)
        {
            // Only log authenticated requests
            var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            //var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            Console.WriteLine($"---------------------------------Middleware {context.Request.Method} {context.Request.Path} | Token: {(string.IsNullOrEmpty(token) ? "MISSING" : "present")}");

            if (!string.IsNullOrEmpty(token))
            {
                if (!sessionService.IsSessionValid(token))
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsJsonAsync(new { error = "Session expired" });
                    return;
                }
                sessionService.UpdateActivity(token);

                var (user, _) = sessionService.Sessions.GetValueOrDefault(token);
                if (user != null)
                {
                    var endpoint = context.Request.Path.Value ?? "unknown";
                    var method = context.Request.Method;
                    var action = $"{method} {endpoint}";

                    await loggingService.LogAsync(
                        userId: user.Id,
                        userRole: user.Role?.Name ?? "User",
                        action: action,
                        endpoint: endpoint,
                        httpMethod: method
                    );
                }
            }

            await _next(context);
        }
    }
}
