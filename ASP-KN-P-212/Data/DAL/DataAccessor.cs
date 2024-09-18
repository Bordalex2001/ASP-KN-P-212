using ASP_KN_P_212.Services.Kdf;

namespace ASP_KN_P_212.Data.DAL
{
    public class DataAccessor
    {
        private readonly Object _dbLocker = new Object();
        private readonly DataContext _dataContext;
        private readonly IKdfService _kdfService;
        public UserDao UserDao { get; private set; }
        public ContentDao ContentDao { get; private set; }

        public DataAccessor(DataContext dataContext, IKdfService kdfService)
        {
            _dataContext = dataContext;
            _kdfService = kdfService;
            UserDao = new UserDao(dataContext, kdfService, _dbLocker);
            ContentDao = new(_dataContext, _dbLocker);
        }
    }
}