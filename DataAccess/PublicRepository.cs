using IdentityServer4.EntityFramework.DbContexts;
using iread_identity_ms.DataAccess.Data;
using iread_identity_ms.DataAccess.Repo;
using iread_identity_ms.Web.Service;
using IdentityServer4.Models;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Identity;
using iread_identity_ms.DataAccess.Data.Entity;

namespace iread_identity_ms.DataAccess
{
    public class PublicRepository:IPublicRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ConfigurationDbContext _configurationContext;
         private readonly PasswordHasher<ApplicationUser> _passwordHasher;
        private IAppUsersRepository _appUsersRepository;


        public PublicRepository( ApplicationDbContext context,
         PasswordHasher<ApplicationUser> passwordHasher,
         ConfigurationDbContext configurationContext
        )
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _configurationContext = configurationContext;
        }

        public IAppUsersRepository GetAppUsersRepository {
            get
            {
                return _appUsersRepository ??= new AppUsersRepository(_context, _passwordHasher, _configurationContext);
            }
        }

    }
}
