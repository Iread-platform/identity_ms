
using System.Collections.Generic;
using System.Threading.Tasks;
using iread_identity_ms.DataAccess;
using iread_identity_ms.DataAccess.Data.Entity;
using iread_identity_ms.DataAccess.Repo;
using Microsoft.EntityFrameworkCore;

namespace iread_identity_ms.Web.Service
{
    public class AppUsersService
    {

        private readonly IPublicRepository _repository;
        private readonly SecurityService _securityService;
        public AppUsersService(IPublicRepository repository, SecurityService securityService)
        {
            _repository = repository;
            _securityService = securityService;
        }

        public async Task<ApplicationUser> GetByEmail(string email)
        {
            return await _repository.GetAppUsersRepository.GetByEmail(email);
        }
        public async Task<List<ApplicationUser>> GetAll()
        {
            return await _repository.GetAppUsersRepository.GetAll();
        }

        public async Task<ApplicationUser> GetById(int id)
        {
            return await _repository.GetAppUsersRepository.GetById(id);
        }

         public bool Insert(ApplicationUser user)
        {
            var hashSalt = _securityService.EncryptPassword(user.Password);
            string plainPassword = user.Password;
            user.Password = hashSalt.Hash;
            user.StoredSalt = hashSalt.Salt;
            try
            {
                _repository.GetAppUsersRepository.Insert(user, plainPassword);
                return true; 
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }

         public bool Delete(ApplicationUser user)
        {
            try
            {
                _repository.GetAppUsersRepository.Delete(user);
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }
    }
}