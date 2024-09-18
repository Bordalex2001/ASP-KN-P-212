using ASP_KN_P_212.Data.DAL;
using ASP_KN_P_212.Data.Entities;
using Microsoft.Extensions.Primitives;
using System.Security.Claims;

namespace ASP_KN_P_212.Middleware
{
    public class AuthTokenMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthTokenMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, DataAccessor dataAccessor)
        {
            /* Це друга авторизація, яка працює поруч з сесійною.
               Дані мають не перезаписуватись, а додаватись */
            StringValues authHeader = context.Request.Headers["Authorization"];
            try
            {
                /* Токени:
                 * Передаються за стандартною схемою - заголовком
                 * Authorization: Bearer 1233242
                 * де 1233242 - токен
                 */
                if (authHeader == StringValues.Empty)
                {
                    throw new Exception("Authentication required");
                }
                String authValue = authHeader.First()!;
                if (!authValue.StartsWith("Bearer "))
                {
                    throw new Exception("Bearer scheme required");
                }
                String token = authValue[7..];   // вилучаємо префікс 'Bearer '
                Guid tokenId;
                try { tokenId = Guid.Parse(token); }
                catch { throw new Exception("Token invalid: GUID required"); }
                User? user = dataAccessor.UserDao.GetUserByToken(tokenId)
                    ?? throw new Exception("Token invalid or expired");

                Claim[] claims = new Claim[] {
                    new(ClaimTypes.Sid,      user.Id.ToString()),
                    new(ClaimTypes.Email,    user.Email),
                    new(ClaimTypes.Name,     user.Name),
                    new(ClaimTypes.UserData, user.AvatarUrl ?? ""),
                    new(ClaimTypes.Role,     user.Role ?? ""),
                    new("EmailConfirmCode",  user.EmailConfirmCode ?? "")
                };

                context.User.AddIdentity(   // ! Додавання ще одної авторизації
                    new ClaimsIdentity(     // їх можна розрізняти за типом, який
                        claims,             // у нас є іменем класу
                        nameof(AuthTokenMiddleware)
                    )
                );
            }
            catch (Exception ex)
            {
                context.Items.Add(new(nameof(AuthTokenMiddleware), ex.Message));
            }
            await _next(context);
        }
    }

    public static class AuthTokenMiddleWareExtension
    {
        public static IApplicationBuilder UseAuthToken(this IApplicationBuilder app)
        {
            return app.UseMiddleware<AuthTokenMiddleware>();
        }
    }
}
/* Авторизація токенами
 * REST -> сесії не використовуємо (для АРІ), кожен запит 
 * авторизуємо окремо
 */
/* REST - Representation State Transfer
 * Набір вимог до архітектури та роботи серверу
 * - відсутність "пам'яті" - кожен запит обробляється незалежно
 *    від історії попередніх запитів
 * - реалізація CRUD
 * - стандартизація/шаблонізація запитів та відповідей
 *    (як запити, так і відповіді мають шаблонну структуру)
 *    наприклад, всі запити методом PATCH спрямовані на повну
 *       деталізацію інформації про об'єкт, авторизація має 
 *       єдину схему, відомості про мову(локалізацію) передаються у
 *       заголовку "Local"
 *    а всі відповіді містять у собі поля з назвою сервісу та час запиту
 *    
 *    PATCH /room/123 <----> { service: "room API", time: "1616816", data: ... }
 *    PATCH /user/123 <----> { service: "user API", time: "1678946", data: ... }
 */