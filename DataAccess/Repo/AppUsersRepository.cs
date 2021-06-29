using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iread_identity_ms.DataAccess.Data;
using iread_identity_ms.DataAccess.Data.Entity;
using Microsoft.EntityFrameworkCore;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Identity;

namespace iread_identity_ms.DataAccess.Repo
{
    public class AppUsersRepository:IAppUsersRepository
    {
         private readonly ApplicationDbContext _context;
         private readonly ConfigurationDbContext _configurationContext;
         private PasswordHasher<ApplicationUser> _passwordHasher;

        public AppUsersRepository(ApplicationDbContext context,
        PasswordHasher<ApplicationUser> passwordHasher,
         ConfigurationDbContext configurationContext)
        {
            _context = context;
            _configurationContext = configurationContext;
            _passwordHasher = passwordHasher;
        }

        public Task<List<ApplicationUser>> GetAll()
        {
            return _context.ApplicationUsers.ToListAsync();
        }
        
        public async Task<ApplicationUser> GetById(string id)
        {
            return await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<ApplicationUser> GetByEmail(string email)
        {
            return await _context.ApplicationUsers.SingleOrDefaultAsync(u => u.Email == email);
        }

        public async Task<ApplicationUser> GetByName(string userName)
        {
            return await _context.ApplicationUsers.SingleOrDefaultAsync(u => u.UserName == userName);
        }

        public void Insert(ApplicationUser user)
        {
                Guid guid = Guid.NewGuid();
                user.Id = guid.ToString();
                user.Name = user.Email;
                user.NormalizedUserName = user.Email;
                _context.ApplicationUsers.Add(user);
                var hasedPassword = _passwordHasher.HashPassword(user, user.Password);
                user.SecurityStamp = Guid.NewGuid().ToString();
                user.PasswordHash = hasedPassword;
                user.UserName = user.Name;
                _context.SaveChanges();
            
        }

        public void Delete(ApplicationUser user)
        {
            
            _context.ApplicationUsers.Remove(user);
            _context.SaveChanges();
        }

        public bool Exists(string id)
        {
            return _context.ApplicationUsers.Any(u => u.Id == id);
        }

        
    }
}
