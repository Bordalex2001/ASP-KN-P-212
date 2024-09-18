using ASP_KN_P_212.Data.Entities;
using ASP_KN_P_212.Services.Kdf;
using Microsoft.EntityFrameworkCore;

namespace ASP_KN_P_212.Data.DAL
{
    public class UserDao
    {
        private readonly DataContext _dataContext;
        private readonly IKdfService _kdfService;
        private readonly Object _dbLocker;

        public UserDao(DataContext dataContext, IKdfService kdfService, object dbLocker)
        {
            _dataContext = dataContext;
            _kdfService = kdfService;
            _dbLocker = dbLocker;
        }

        public User? GetUserById(String id)
        {
            User? user;
            try
            {
                lock (_dbLocker)
                {
                    user = _dataContext.Users.Find(Guid.Parse(id));
                }
            }
            catch { return null; }
            return user;
        }

        public User? GetUserByToken(Guid token)
        {
            User? user;
            lock (_dbLocker)
            {
                user = _dataContext.Tokens
                    .Include(t => t.User)
                    .FirstOrDefault(t => t.Id == token)
                    ?.User;
            }
            return user;
        }

        public Token? CreateTokenForUser(User user)
        {
            return CreateTokenForUser(user.Id);
        }

        public Token? CreateTokenForUser(Guid userId)
        {
            Token token = new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SubmitDt = DateTime.Now,
                ExpireDt = DateTime.Now.AddDays(1),
            };
            _dataContext.Tokens.Add(token);
            try
            {
                lock (_dbLocker)
                {
                    _dataContext.SaveChanges();
                }
                return token;
            }
            catch
            {
                _dataContext.Tokens.Remove(token);
                return null;
            }
        }

        public User? Authorize(String email, String password)
        {
            User? user = _dataContext
                .Users
                .FirstOrDefault(u => u.Email == email);

            if (user == null || 
                user.DerivedKey != _kdfService.DerivedKey(user.Salt, password))
            {
                return null;
            }
            return user;
        }

        public void Signup(User user)
        {
            if(user.Id == default)
            {
                user.Id = Guid.NewGuid();
            }
            _dataContext.Users.Add(user);
            _dataContext.SaveChanges();
        }

        public Boolean ConfirmEmail(String email, String code)
        {
            // Find User by E-mail
            User? user;
            lock (_dbLocker)
            {
                user = _dataContext.Users.FirstOrDefault(u => u.Email == email);
            }
            if (user == null || user.EmailConfirmCode != code)
            {
                return false;
            }
            user.EmailConfirmCode = null;
            lock (_dbLocker)
            {
                _dataContext.SaveChanges();
            }
            return true;
        }
    }
}
/* DAL - Data Access Layer - сукупність усіх DAO
 * DAO - Data Access Object - набір методів для роботи з сутністю
 */