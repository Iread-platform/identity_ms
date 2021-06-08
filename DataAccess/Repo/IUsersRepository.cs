using System.Collections.Generic;
using System.Threading.Tasks;
using iread_identity_ms.DataAccess.Data.Entity;

namespace iread_identity_ms.DataAccess.Repo
{
    public interface IUsersRepository
    {
        public Task<SysUser> GetById(int id);

        public Task<SysUser> GetByEmail(string email);

        public void Insert(SysUser user);

        public Task<List<SysUser>> GetAll();

        public void Delete(SysUser user);

        public bool Exists(int id);

    }
}
