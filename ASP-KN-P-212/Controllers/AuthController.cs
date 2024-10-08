﻿using ASP_KN_P_212.Data.DAL;
using ASP_KN_P_212.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASP_KN_P_212.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DataAccessor _dataAccessor;

        public AuthController(DataAccessor dataAccessor)
        {
            _dataAccessor = dataAccessor;
        }

        [HttpGet]
        public object Get([FromQuery(Name="e-mail")] String email, String? password)
        {
            User? user = _dataAccessor.UserDao.Authorize(email, password ?? "");
            if (user == null)
            {
                Response.StatusCode = StatusCodes.Status401Unauthorized;
                return new { Status = "Auth failed" };
            }
            else
            {
                /* Http-сесії -- спосіб для збереження з боку сервера даних, що 
                 будуть доступними після перевантаження сторінки. 
                 Налаштування - https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state?view=aspnetcore-8.0
                 Після налаштування сесії доступні через властивість HttpContext
                 Ідея сесій - збереження даних у формі "ключ-значення", причому 
                   значення має бути серіалізовуваним (таким, що можна зберегти у файл)
                 */
                HttpContext.Session.SetString("auth-user-id", user.Id.ToString());

                return user;
            }
        }

        [HttpPost]
        public object Post()
        {
            return new { Status = "POST works" };
        }
        
        [HttpPut]
        public object Put()
        {
            return new { Status = "PUT works" };
        }

        [HttpPatch]
        public object Patch(String email, String code)   // Confirm E-mail by code
        {
            if (_dataAccessor.UserDao.ConfirmEmail(email, code))
            {
                Response.StatusCode = StatusCodes.Status202Accepted;
                return new { Status = "OK" };
            }
            else
            {
                Response.StatusCode = StatusCodes.Status409Conflict;
                return new { Status = "Error" };
            }
        }

        [HttpGet("token")]
        public Token? GetToken(String email, String? password)
        {
            var user = _dataAccessor.UserDao.Authorize(email, password ?? "");
            if (user == null)
            {
                Response.StatusCode = StatusCodes.Status401Unauthorized;
                return null;
            }
            return _dataAccessor.UserDao.CreateTokenForUser(user);
        }
    }
}
/* Схеми авторизації API. Токени
 * Розрізняють дві групи схем
 * - серверні сесії - підходить для Server-Page архітектури
 * - токени - для SPA архітектури
 * Токен (від англ. - жетон, посвідчення) - дані, що дозволяють
 * автентифікувати запит від фронтенду
 * 
 * Back             Front
 *   <------------[login,password]
 * [token: 123]-------->
 *    <-----------[GET /rooms token: 123]
 * [перевірка
 *  токена,
 *  відповідь]---->
 *  
 *  
 */

/* Контролери поділяються на дві групи - API та MVC
 * MVC:
 *  - мають багато Action, кожен з яких запускається своїм Route
 *      /Home/Ioc  -->  public ViewResult Ioc() {...}
 *      /Home/Form -->  public ViewResult Form() {...}
 *    при цьому метод запиту ролі не грає, можливо лише обмежити їх перелік
 *    GET /Home/Ioc,  POST /Home/Ioc  --> public ViewResult Ioc()
 *  - повернення - IActionResult, частіше за все ViewResult
 *      
 * API:
 *  - мають одну адресу [Route("api/auth")], а різні дії запускаються
 *     різними методами запитів
 *     GET  api/auth  --> public object Get()
 *     POST api/auth  --> public object Post()
 *     PUT  api/auth  --> public object Put() 
 *   вся відмінність - у методі запиту, неможна потрапити до одного Action
 *   різними методами
 * - повернення - дані, які автоматично перетворюються або в текст, або
 *    в JSON (якщо тип повернення String - text/plain, якщо інший
 *    object, List<...>, User  -- application/json )
 *      
 *      
 *      
 * [FromQuery]
 *                /api/auth?email=i.ua&password=123
 *                            |             |  зв'язування параметрів - за іменами
 * public object Get(String email, String password)
 *   1) запит (його частина - query) аналізується і зв'язується
 *      з параметрами public object Get(...) за збігом імен
 *   2) якщо якійсь з необхідних параметрів не знайдено, то автоматично
 *      формується відповідь 400 (Bad request)
 *      Необхідні параметри - тип яких не містить Nullability (?)
 *      
 *                          /api/auth?e-mail=i.ua&password=123
 *                                       |
 * public object Get([FromQuery(Name="e-mail")] String email, String password)
 */