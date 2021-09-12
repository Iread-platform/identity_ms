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
    public class AppUsersRepository : IAppUsersRepository
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
            var hashedPassword = _passwordHasher.HashPassword(user, user.PasswordHash);
            user.SecurityStamp = Guid.NewGuid().ToString();
            user.PasswordHash = hashedPassword;
            user.UserName = user.Name;
            _context.ApplicationUsers.Add(user);
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

        public async Task<List<ApplicationUser>> GetByRole(string role)
        {
            return await _context.ApplicationUsers.Where(u => u.Role == role).ToListAsync();
        }

        public void ResetPassword(ApplicationUser user, string newPassword)
        {
            var hashedPassword = _passwordHasher.HashPassword(user, newPassword);
            user.SecurityStamp = Guid.NewGuid().ToString();
            user.PasswordHash = hashedPassword;
            _context.ApplicationUsers.Update(user);
            _context.SaveChanges();
        }

        public void Update(ApplicationUser newUser, ApplicationUser oldUser)
        {
            oldUser.Avatar = newUser.Avatar;
            oldUser.Level = newUser.Level;
            oldUser.FirstName = newUser.FirstName;
            oldUser.LastName = newUser.LastName;
            oldUser.BirthDay = newUser.BirthDay;
            oldUser.CustomPhoto = newUser.CustomPhoto;
            oldUser.Email = newUser.Email;
            _context.ApplicationUsers.Update(oldUser);
            _context.SaveChanges();
        }
    }
}
