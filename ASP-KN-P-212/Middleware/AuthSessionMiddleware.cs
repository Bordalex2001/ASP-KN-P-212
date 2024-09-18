using ASP_KN_P_212.Data.DAL;
using ASP_KN_P_212.Middleware;
using System.Globalization;
using System.Security.Claims;

namespace ASP_KN_P_212.Middleware
{
    public class AuthSessionMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthSessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // В силу особливостей роботи Middleware, інжекція сервісів здійснюється не у 
        // конструктор, а в параметри методу InvokeAsync
        public async Task InvokeAsync(HttpContext context, DataAccessor dataAccessor)
        {
            // "прямий хід" - від запиту до Razor
            // чи не запитано вихід
            if (context.Request.Query.ContainsKey("logout"))
            {
                context.Session.Remove("auth-user-id");
                context.Response.Redirect("/");
                return;  // без _next це припинить роботу
            }
            else if (context.Session.GetString("auth-user-id") is String userId)
            {
                var user = dataAccessor.UserDao.GetUserById(userId);
                if (user != null)
                {
                    // Система авторизації ASP передбачає заповнення спеціального поля
                    // context.User - набору Claims-параметрів, кожен з яких відповідає
                    // за свій атрибут (id, email, ...)
                    Claim[] claims = new Claim[] {
                        new(ClaimTypes.Sid,      userId),
                        new(ClaimTypes.Email,    user.Email),
                        new(ClaimTypes.Name,     user.Name),
                        new(ClaimTypes.UserData, user.AvatarUrl ?? ""),
                        new(ClaimTypes.Role, user.Role ?? ""),
                        new("EmailConfirmCode", user.EmailConfirmCode ?? "")
                    };

                    context.User = new ClaimsPrincipal(
                        new ClaimsIdentity(
                            claims,
                            nameof(AuthSessionMiddleware)
                        )
                    );
                }
            }
            await _next(context);
            // "зворотній хід" - від Razor до відповіді
        }
    }

    // Традиція - робити розширення, які замість app.UseMiddleware<AuthSessionMiddleware>();
    // дозволять використовувати скорочену форму на кшталт app.UseAuthSession()
    public static class AuthSessionMiddlewareExtension
    {
        public static IApplicationBuilder UseAuthSession(this IApplicationBuilder app)
        {
            return app.UseMiddleware<AuthSessionMiddleware>();
        }
    }
}
/* Middleware створюється один раз при запуску проєкту, порядок виконання
 * об'єктів визначається з Program.cs та ядра ASP і кожен об'єкт Middleware
 * одержує як залежність RequestDelegate _next -- посилання на наступний
 * об'єкт, до якого слід передати управління після своєї роботи. 
 */