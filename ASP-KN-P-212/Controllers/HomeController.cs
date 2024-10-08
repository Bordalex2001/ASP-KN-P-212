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
        /* �������� ������ (�����������) - ����� � ����������
        �� �������� �������� �� �������� ��'����. �������
        �������������� ����� �������� - ����� �����������. �� 
        ��������, ��-�����, ��������� ���� �� ������� (readonly) ��,
        ��-�����, ������������ ��������� ��'���� ��� ��������
        �����������. � ���������� ����� �������� ������������� 
        �� ����� ��������� (_logger)
        */
        private readonly ILogger<HomeController> _logger;
        // ��������� ���� ��� ��������� �� �����
        private readonly IHashService _hashService;
        private readonly IRandomService _randomService;

        // �������� ��������� ����� - ���� � �� ������, �� ���� ������
        private readonly DataContext _dataContext;
        private readonly DataAccessor _dataAccessor;
        private readonly IKdfService _kdfService;
        private readonly IEmailService _emailService;

        // ������ �� ������������ ��������-���������� � �������� �� � ��
        public HomeController(ILogger<HomeController> logger, IHashService hashService, IRandomService randomService, DataContext dataContext, DataAccessor dataAccessor, IKdfService kdfService, IEmailService emailService)
        {
            _logger = logger;           // ���������� ��������� �����������, �� ��
            _hashService = hashService; // ������ ��������� ��� ��������� ����������
            _randomService = randomService;
            _dataContext = dataContext;
            _dataAccessor = dataAccessor;
            _kdfService = kdfService;
            _emailService = emailService;
        }

        public IActionResult ConfirmEmail(String id)
        {
            /* ���� Basic-�������������� -- ��������� ����� �� ������
             * ����� ":" �� ��������� � Base64
             * user@i.ua:123  ---  dXNlckBpLnVhOjEyMw==*/
            String email, code;
            try
            {
                String data = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(id));
                String[] parts = data.Split(':', 2);
                email = parts[0];
                code = parts[1];
                ViewData["result"] = _dataAccessor.UserDao.ConfirmEmail(email, code) ? "����� ������ �����������" : "������� ������������ �����";
            }
            catch
            {
                ViewData["result"] = "���� �� ���������";
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
                Title = "������� ���������� ���������",
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

        // ������ ����� ����������� ���������� ������, ����������-�����������
        public IActionResult Model(Models.Home.Model.FormModel? formModel)
        {
            // ������ ������������� ����������� � ������������ ���������
            Models.Home.Model.PageModel pageModel = new()
            {
                TabHeader = "�����",
                PageTitle = "����� � ASP",
                FormModel = formModel
            };
            // ������ ������������� ���������� ���������� View()
            return View(pageModel);
        }

        [HttpPost]  // ������� �������� ����� ���� POST-�������
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
                {   // ���� ������� �������� - �������� �����������
                    // ���������� E-mail
                    // �������� ���
                    String code = Guid.NewGuid().ToString()[..6];
                    String slug = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{formModel.UserEmail}:{code}"));
                    MailMessage mailMessage = new()
                    {
                        Subject = "ϳ����������� �����",
                        IsBodyHtml = true,
                        Body = "<p>��� ������������ ����� ������ �� ���� ���</p>" + 
                        $"<h2 style='color: orange'>{code}</h2>" + 
                        $"<p>��� �������� �� <a href='{Request.Scheme}://{Request.Host}/Home/ConfirmEmail/{slug}'>��� ����������</a></p>"
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
                        pageModel.ValidationErrors["email"] = "�� ������� �������� E-mail";
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
                    // � ����, �������� ���� 
                    // ĳ������� ���������� �����:
                    int dotPosition = model.UserAvatar.FileName.LastIndexOf('.');
                    if (dotPosition == -1)
                    {
                        result[nameof(model.UserAvatar)] = "File without extension not allowed";
                    }
                    String ext = model.UserAvatar.FileName[dotPosition..];
                    // ��������� ���������� �� �������� �����.
                    // ...
                    // _logger.LogInformation(ext);
                    // �������� ���� � /wwwroot/img/avatars � ����� ������
                    // (�������� ����� �������� �� �����)
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
/* �.�. ���������� ��������� ��������� � ������� �������� �����
 * ���������� ������ ��������� (���������) �� ������������ 
 * �������� �� ���������� (����������) -- ��������� ��������
 * �������� ����������� �� ����� ������������
 *   
 */
/* �.�. ���������� ������ "�'�����" ��������� 
 * �� ���������� ��������� �����
 * � �������� (���������) �����
 */