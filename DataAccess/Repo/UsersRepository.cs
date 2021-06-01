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
    public class UsersRepository:IUsersRepository
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

        public async Task<SysUser> GetByEmail(string email)
        {
            return await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
        }

        public void Insert(SysUser user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public void Delete(SysUser user)
        {
            _context.Users.Remove(user);
            _context.SaveChanges();
        }

        public bool Exists(int id)
        {
            return _context.Users.Any(u => u.UserId == id);
        }

    }
}
