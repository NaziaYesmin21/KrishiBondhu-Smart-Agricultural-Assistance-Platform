using Microsoft.AspNetCore.Http;

namespace KrishiBondhu___Smart_Agricultural_Assistance_Platform.Middleware
{
    public class CustomAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public CustomAuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();

            // Pages that do not require login
            bool isPublicPage =
     path == "/auth/login" ||
     path == "/auth/register" ||
     path == "/auth/logout";

            // Check whether user is logged in
            var userId = context.Session.GetInt32("UserId");

            // If user is not logged in, redirect to Login
            if (!isPublicPage && userId == null)
            {
                context.Response.Redirect("/Auth/Login");
                return;
            }

            // Prevent browser from caching protected pages
            if (!isPublicPage)
            {
                context.Response.Headers["Cache-Control"] =
                    "no-cache, no-store, must-revalidate";

                context.Response.Headers["Pragma"] = "no-cache";

                context.Response.Headers["Expires"] = "0";
            }

            await _next(context);
        }
    }
}