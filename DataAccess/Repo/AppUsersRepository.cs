﻿using System;
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
    public class AppUsersRepository:IAppUsersRepository
    {
         private readonly ApplicationDbContext _context;
         private readonly ConfigurationDbContext _configurationContext;
        private readonly SecurityService _securityService;
        public AppUsersRepository(ApplicationDbContext context, SecurityService securityService, ConfigurationDbContext configurationContext)
        {
            _context = context;
            _securityService = securityService;
            _configurationContext = configurationContext;
        }

        public Task<List<ApplicationUser>> GetAll()
        {
            return _context.ApplicationUsers.ToListAsync();
        }
        
        public async Task<ApplicationUser> GetById(int id)
        {
            return await _context.ApplicationUsers.FindAsync(id);
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

        
    }
}
