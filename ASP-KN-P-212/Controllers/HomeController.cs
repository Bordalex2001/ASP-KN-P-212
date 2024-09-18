using ASP_KN_P_212.Data;
using ASP_KN_P_212.Data.DAL;
using ASP_KN_P_212.Models;
using ASP_KN_P_212.Models.Home.FrontendForm;
using ASP_KN_P_212.Models.Home.Ioc;
using ASP_KN_P_212.Models.Home.Random;
using ASP_KN_P_212.Models.Home.Signup;
using ASP_KN_P_212.Services.Email;
using ASP_KN_P_212.Services.Hash;
using ASP_KN_P_212.Services.Kdf;
using ASP_KN_P_212.Services.Random;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.Mail;

namespace ASP_KN_P_212.Controllers
{
    public class HomeController : Controller
    {
        /* Інжекція сервісів (залежностей) - запит у контейнера
        на передачу посилань на відповідні об'єкти. Найбільш
        рекомендований спосіб інжекції - через конструктор. Це 
        дозволяє, по-перше, оголосити поля як незмінні (readonly) та,
        по-друге, унеможливити створення об'єктів без передачі
        залежностей. У стартовому проєкті інжекція демонструється 
        на сервісі логування (_logger)
        */
        private readonly ILogger<HomeController> _logger;
        // створюємо поле для посилання на сервіс
        private readonly IHashService _hashService;
        private readonly IRandomService _randomService;

        // інжекція контексту даних - така ж за формою, як інші сервіси
        private readonly DataContext _dataContext;
        private readonly DataAccessor _dataAccessor;
        private readonly IKdfService _kdfService;
        private readonly IEmailService _emailService;

        // додаємо до конструктора параметр-залежність і зберігаємо її у тілі
        public HomeController(ILogger<HomeController> logger, IHashService hashService, IRandomService randomService, DataContext dataContext, DataAccessor dataAccessor, IKdfService kdfService, IEmailService emailService)
        {
            _logger = logger;           // Збереження переданих залежностей, що їх
            _hashService = hashService; // передає контейнер при створенні контролера
            _randomService = randomService;
            _dataContext = dataContext;
            _dataAccessor = dataAccessor;
            _kdfService = kdfService;
            _emailService = emailService;
        }

        public IActionResult ConfirmEmail(String id)
        {
            /* Ідея Basic-автентифікації -- розділення логіну та паролю
             * через ":" та кодування у Base64
             * user@i.ua:123  ---  dXNlckBpLnVhOjEyMw==*/
            String email, code;
            try
            {
                String data = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(id));
                String[] parts = data.Split(':', 2);
                email = parts[0];
                code = parts[1];
                ViewData["result"] = _dataAccessor.UserDao.ConfirmEmail(email, code) ? "Пошта успішно підтверджена" : "Помилка підтвердження пошти";
            }
            catch
            {
                ViewData["result"] = "Дані не розпізнані";
            }
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }
        
        public IActionResult Intro()
        {
            return View();
        }
        
        public IActionResult Url()
        {
            return View();
        }
        
        public IActionResult Data()
        {
            ViewData["users-count"] = _dataContext.Users.Count();
            return View();
        }
        
        public IActionResult Ioc(String? format)  // Inversion of Control
        {
            IocPageModel pageModel = new()
            {
                Title = "Декілька випадкових дайджестів",
                SingleHash = _hashService.Digest("Hello, World!")
            };
            for (int i = 0; i < 5; i++)
            {
                String str = (i + 100500).ToString();
                pageModel.Hashes[str] = _hashService.Digest(str);
            }
            if(format == "json")
            {
                return Json(pageModel);
            }
            return View(pageModel);
        }
        
        public IActionResult AboutRazor()
        {
            RandomPageModel pageModel = new()
            {
                RandomStrings = []
            };
            for (int i = 0; i < 10; i++)
            {
                String? otp = _randomService.GenerateOtp(6);
                String? filename = _randomService.GenerateFilename(10);
                String? salt = _randomService.GenerateSalt(8);

                pageModel.RandomStrings.Add(new RandomStringModel
                {
                    Otp = otp,
                    Filename = filename,
                    Salt = salt
                });
            }
            return View(pageModel);
        }

        /*public IActionResult Random()
        {
            return View(pageModel);
        }*/

        public ViewResult Admin()
        {
            return View();
        }

        // Модель форми зазначається параметром методу, заповнення-автоматичне
        public IActionResult Model(Models.Home.Model.FormModel? formModel)
        {
            // Модель представлення створюється і заповнюється самостійно
            Models.Home.Model.PageModel pageModel = new()
            {
                TabHeader = "Моделі",
                PageTitle = "Моделі в ASP",
                FormModel = formModel
            };
            // модель представлення передається аргументом View()
            return View(pageModel);
        }

        [HttpPost]  // атрибут дозволяє запит лише POST-методом
        public JsonResult FrontendForm([FromBody] FrontendFormInput input)
        {
            FrontendFormOutput output = new()
            {
                Code = 200,
                Message = $"{input.UserName} -- {input.UserEmail}"
            };
            _logger.LogInformation(output.Message);
            return Json(output);
        }

        public IActionResult Signup(SignupFormModel? formModel)
        {
            SignupPageModel pageModel = new()
            {
                FormModel = formModel
            };
            if(formModel?.HasData ?? false)
            {
                pageModel.ValidationErrors = _ValidateSignupModel(formModel);
                if(pageModel.ValidationErrors.Count == 0)
                {   // Немає помилок валідації - реєструємо користувача
                    // Перевіряємо E-mail
                    // Генеруємо код
                    String code = Guid.NewGuid().ToString()[..6];
                    String slug = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{formModel.UserEmail}:{code}"));
                    MailMessage mailMessage = new()
                    {
                        Subject = "Підтвердження пошти",
                        IsBodyHtml = true,
                        Body = "<p>Для підтвердження пошти введіть на сайті код</p>" + 
                        $"<h2 style='color: orange'>{code}</h2>" + 
                        $"<p>Або перейдіть за <a href='{Request.Scheme}://{Request.Host}/Home/ConfirmEmail/{slug}'>цим посиланням</a></p>"
                    };
                    _logger.LogInformation(mailMessage.Body);
                    mailMessage.To.Add(formModel.UserEmail);
                    try
                    {
                        _emailService.Send(mailMessage);
                        String salt = Guid.NewGuid().ToString();
                        _dataAccessor.UserDao.Signup(new()
                        {
                            Name = formModel.UserName,
                            Email = formModel.UserEmail,
                            EmailConfirmCode = code,
                            Birthdate = formModel.UserBirthdate,
                            AvatarUrl = formModel.SavedAvatarFilename,
                            Salt = salt,
                            DerivedKey = _kdfService.DerivedKey(salt, formModel.Password)
                        });
                    }
                    catch (Exception ex)
                    {
                        pageModel.ValidationErrors["email"] = "Не вдалось надіслати E-mail";
                        _logger.LogInformation(ex.Message);
                    }
                }
            }
            // _logger.LogInformation(Directory.GetCurrentDirectory());
            return View(pageModel);
        }
        
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private Dictionary<string, string> _ValidateSignupModel(SignupFormModel? model)
        {
            Dictionary<string, string> result = new();

            if (model == null)
            {
                result["model"] = "Model is null";
            }
            else
            {
                if (String.IsNullOrEmpty(model.UserName))
                {
                    result[nameof(model.UserName)] = "User Name should not be empty";
                }
                if (String.IsNullOrEmpty(model.UserEmail))
                {
                    result[nameof(model.UserEmail)] = "User Email should not be empty";
                }
                if (model.UserBirthdate == default(DateTime))
                {
                    result[nameof(model.UserBirthdate)] = "User Birthdate should not be empty";
                }
                if (model.UserAvatar != null)
                {
                    // є файл, аналізуємо його 
                    // Дізнаємось розширення файлу:
                    int dotPosition = model.UserAvatar.FileName.LastIndexOf('.');
                    if (dotPosition == -1)
                    {
                        result[nameof(model.UserAvatar)] = "File without extension not allowed";
                    }
                    String ext = model.UserAvatar.FileName[dotPosition..];
                    // Перевірити розширення на графічні файли.
                    // ...
                    // _logger.LogInformation(ext);
                    // зберігаємо файл у /wwwroot/img/avatars з новим іменем
                    // (передані імена залишати не варто)
                    String path = Directory.GetCurrentDirectory() + "/wwwroot/images/avatars/";
                    _logger.LogInformation(path);
                    String fileName;
                    String pathName;
                    do
                    {
                        fileName = Guid.NewGuid() + ext;
                        pathName = path + fileName;
                    }
                    while(System.IO.File.Exists(pathName));

                    using FileStream stream = System.IO.File.OpenWrite(pathName);
                    model.UserAvatar.CopyTo(stream);

                    model.SavedAvatarFilename = fileName;
                }
            }
            return result;
        }
    }
}
/* Д.З. Впровадити управління контентом у власний курсовий проєкт
 * Реалізувати засоби створення (додавання) та розмежування 
 * контенту за належністю (авторством) -- можливості власника
 * контенту відрізняються від інших користувачів
 *   
 */
/* Д.З. Реалізувати засоби "м'якого" видалення 
 * та відновлення видалених даних
 * у власному (курсовому) проєкті
 */