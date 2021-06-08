using IdentityServer4.EntityFramework.DbContexts;
using iread_identity_ms.DataAccess.Data;
using iread_identity_ms.DataAccess.Repo;
using iread_identity_ms.Web.Service;
using IdentityServer4.Models;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.EntityFramework.DbContexts;

namespace iread_identity_ms.DataAccess
{
    public class PublicRepository:IPublicRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ConfigurationDbContext _configurationContext;
        private readonly SecurityService _securityService;
        private IUsersRepository _usersRepository;

        public PublicRepository(ApplicationDbContext context, SecurityService securityService, ConfigurationDbContext configurationContext)
        {
            _context = context;
            _securityService = securityService;
            _configurationContext = configurationContext;
        }

        public IUsersRepository getUsersRepository {
            get
            {
                return _usersRepository ??= new UsersRepository(_context,_securityService, _configurationContext);
            }
        }

    }
}
