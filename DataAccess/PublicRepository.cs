using IdentityServer4.EntityFramework.DbContexts;
using iread_identity_ms.DataAccess.Data;
using iread_identity_ms.DataAccess.Repo;
using iread_identity_ms.Web.Service;
using IdentityServer4.Models;
using IdentityServer4.EntityFramework.Mappers;

namespace iread_identity_ms.DataAccess
{
    public class PublicRepository:IPublicRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ConfigurationDbContext _configurationContext;
        private readonly SecurityService _securityService;
        private IUsersRepository _usersRepository;
        private IAppUsersRepository _appUsersRepository;

        public PublicRepository( ApplicationDbContext context, SecurityService securityService
        , ConfigurationDbContext configurationContext
        )
        {
            _context = context;
            _securityService = securityService;
            _configurationContext = configurationContext;
        }

        public IUsersRepository GetUsersRepository {
            get
            {
                return _usersRepository ??= new UsersRepository(_context,_securityService
                //, _configurationContext
                );
            }
        }
        public IAppUsersRepository GetAppUsersRepository {
            get
            {
                return _appUsersRepository ??= new AppUsersRepository(_context,_securityService, _configurationContext);
            }
        }

    }
}
