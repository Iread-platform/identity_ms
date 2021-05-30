using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iread_identity_ms.DataAccess.Data;
using iread_identity_ms.DataAccess.Data.Entity;
using iread_identity_ms.Web.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;


namespace iread_identity_ms.DataAccess.Repo
{
    public class UsersRepository
    {
        private readonly AppDbContext _context;
        private readonly SecurityService _securityService;
        public UsersRepository(AppDbContext context, SecurityService securityService)
        {
            _context = context;
            _securityService = securityService;
        }

        public Task<List<SysUser>> GetAll()
        {
            return _context.Users.ToListAsync();
        }
        
        
        public async Task<SysUser> GetById(int id)
        {
            return await _context.Users.SingleOrDefaultAsync(u => u.UserId == id);
        }

        public bool Update(SysUser user)
        {
            try
            {
                _context.Entry(user).State = EntityState.Modified;
                _context.Entry(user).Property(u => u.Password).IsModified = false;
                _context.Entry(user).Property(u => u.StoredSalt).IsModified = false;

                _context.SaveChangesAsync();

            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            return true;
        }

        public bool ChangePassword(string newPassword, SysUser user)
        {
            try
            {
                var hashSalt = _securityService.EncryptPassword(newPassword);
                user.Password = hashSalt.Hash;
                user.StoredSalt = hashSalt.Salt;
                _context.Entry(user).Property(x => x.Password).IsModified = true;
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            return true;
        }

        public async Task<SysUser> GetByEmail(string email)
        {
            return await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
        }

        public SysUser GetByEmailAndPassword(string email, string pw)
        {
            return _context.Users.SingleOrDefault(u => u.Password == pw && u.Email == email);
        }

        public void Insert(SysUser user)
        {
            _context.Users.Add(user);
        }

        public void Delete(SysUser user)
        {
            _context.Users.Remove(user);
        }

        public bool Exists(int id)
        {
            return _context.Users.Any(u => u.UserId == id);
        }

    }
}
