using System.Collections.Generic;
using System.Threading.Tasks;
using iread_identity_ms.DataAccess.Data.Entity;

namespace iread_identity_ms.DataAccess.Repo
{
    public interface IAppUsersRepository
    {
        public Task<ApplicationUser> GetById(string id);

        public Task<ApplicationUser> GetByEmail(string email);

        public void Insert(ApplicationUser user);

        public Task<List<ApplicationUser>> GetAll();

        public void Delete(ApplicationUser user);

        public bool Exists(string id);
        public Task<ApplicationUser> GetByName(string userName);
        public Task<List<ApplicationUser>> GetByRole(string role);
        public void ResetPassword(ApplicationUser user, string newPassword);
        public void Update(ApplicationUser newUser, ApplicationUser oldUser);
    }
}
