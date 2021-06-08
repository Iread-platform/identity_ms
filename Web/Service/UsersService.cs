
using System.Collections.Generic;
using System.Threading.Tasks;
using iread_identity_ms.DataAccess;
using iread_identity_ms.DataAccess.Data.Entity;
using iread_identity_ms.DataAccess.Repo;
using Microsoft.EntityFrameworkCore;

namespace iread_identity_ms.Web.Service
{
    public class UsersService
    {


        private readonly IPublicRepository _repository;
        private readonly SecurityService _securityService;
        public UsersService(IPublicRepository repository, SecurityService securityService)
        {
            _repository = repository;
            _securityService = securityService;
        }

        public async Task<SysUser> GetByEmail(string email)
        {
            return await _repository.getUsersRepository.GetByEmail(email);
        }
        public async Task<List<SysUser>> GetAll()
        {
            return await _repository.getUsersRepository.GetAll();
        }

        public async Task<SysUser> GetById(int id)
        {
            return await _repository.getUsersRepository.GetById(id);
        }

         public bool Insert(SysUser user)
        {
            var hashSalt = _securityService.EncryptPassword(user.Password);
            string plainPassword = user.Password;
            user.Password = hashSalt.Hash;
            user.StoredSalt = hashSalt.Salt;
            try
            {
                _repository.getUsersRepository.Insert(user, plainPassword);
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
                _repository.getUsersRepository.Delete(user);
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }
    }
}