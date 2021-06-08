using iread_identity_ms.DataAccess.Data;
using iread_identity_ms.DataAccess.Repo;
using iread_identity_ms.Web.Service;

namespace iread_identity_ms.DataAccess
{
    public class PublicRepository:IPublicRepository
    {
        private readonly AppDbContext _context;
        private readonly SecurityService _securityService;
        private IUsersRepository _usersRepository;

        public PublicRepository(AppDbContext context, SecurityService securityService)
        {
            _context = context;
            _securityService = securityService;
        }

        public IUsersRepository getUsersRepository {
            get
            {
                return _usersRepository ??= new UsersRepository(_context,_securityService);
            }
        }

    }
}
