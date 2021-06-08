using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.Entities;
using iread_identity_ms.DataAccess.Data;
using iread_identity_ms.DataAccess.Data.Entity;
using iread_identity_ms.Web.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using IdentityServer4.Models;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.Extensions.DependencyInjection;
using iread_identity_ms.Web.Util;

namespace iread_identity_ms.DataAccess.Repo
{
    public class UsersRepository:IUsersRepository
    {
        //private readonly AppDbContext _context;
         private readonly ApplicationDbContext _context;
         private readonly ConfigurationDbContext _configurationContext;
        private readonly SecurityService _securityService;
        public UsersRepository(ApplicationDbContext context, SecurityService securityService, ConfigurationDbContext configurationContext)
        {
            _context = context;
            _securityService = securityService;
            _configurationContext = configurationContext;
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

        public void Insert(SysUser user, string plainPassword)
        {
            _context.Users.Add(user);
            _context.SaveChanges();

            var client = new IdentityServer4.Models.Client
            {
                ClientId = user.Name,
                ClientName = user.Name,
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets = new List<IdentityServer4.Models.Secret> {new IdentityServer4.Models.Secret(plainPassword.Sha256())}, 
                AllowedScopes = new List<string> {user.Role}
            };

            _configurationContext.Clients.Add(client.ToEntity());
            _configurationContext.SaveChanges();
            
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
