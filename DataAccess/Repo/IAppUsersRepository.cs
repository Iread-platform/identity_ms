using System.Collections.Generic;
using System.Threading.Tasks;
using iread_identity_ms.DataAccess.Data.Entity;

namespace iread_identity_ms.DataAccess.Repo
{
    public interface IAppUsersRepository
    {
        public Task<ApplicationUser> GetById(int id);

        public Task<ApplicationUser> GetByEmail(string email);

        public void Insert(ApplicationUser user, string plainPassword);

        public Task<List<ApplicationUser>> GetAll();

        public void Delete(ApplicationUser user);

        public bool Exists(string id);

    }
}
