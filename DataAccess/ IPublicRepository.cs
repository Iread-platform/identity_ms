using iread_identity_ms.DataAccess.Repo;

namespace iread_identity_ms.DataAccess
{
    public interface IPublicRepository
    {
        IAppUsersRepository GetAppUsersRepository { get; }
    }
}
