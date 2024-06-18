
namespace MailBackend.Middleware
{
    public class SecureEndpointMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string API_KEY_HEADER_NAME = "X-API-KEY";
        private readonly IConfiguration _configuration;
        public SecureEndpointMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }   

        public async Task InvokeAsync(HttpContext ctx)
        {
            if(!ctx.Request.Headers.TryGetValue(API_KEY_HEADER_NAME, out var inputKey))
            {
                ctx.Response.StatusCode = 401; //unauthorized
                await ctx.Response.WriteAsync("Invalid Headers");
                return;
            }

            string secret = Environment.GetEnvironmentVariable("API_KEY");
            if(string.IsNullOrEmpty(secret) || !secret.Equals(inputKey))
            {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync("Unauthorized Client");
                return;
            }

            await _next(ctx);

        }
    }
}
