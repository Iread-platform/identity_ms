
using System.Collections.Generic;
using System.Threading.Tasks;
using iread_identity_ms.DataAccess.Data.Entity;
using iread_identity_ms.DataAccess.Repo;
using Microsoft.EntityFrameworkCore;

namespace iread_identity_ms.Web.Service
{
    public class UsersService
    {
        private readonly UsersRepository _usersRepository;
        private readonly SecurityService _securityService;
        public UsersService(UsersRepository usersRepository, SecurityService securityService)
        {
            _usersRepository = usersRepository;
            _securityService = securityService;
        }

        public async Task<SysUser> GetByEmail(string email)
        {
            return await _usersRepository.GetByEmail(email);
        }
        public async Task<List<SysUser>> GetAll()
        {
            return await _usersRepository.GetAll();
        }

        public async Task<SysUser> GetById(int id)
        {
            return await _usersRepository.GetById(id);
        }

         public bool Insert(SysUser user)
        {
            var hashSalt = _securityService.EncryptPassword(user.Password);
            user.Password = hashSalt.Hash;
            user.StoredSalt = hashSalt.Salt;
            try
            {
                _usersRepository.Insert(user);
                return true; 
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }

         public bool Delete(SysUser user)
        {
            try
            {
                _usersRepository.Delete(user);
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }
    }
}