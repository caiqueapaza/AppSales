using APISales.Domain.Users;

namespace APISales.Infrastructure.Repositories
{
    public interface IUserRepository
    {
        IEnumerable<User> GetUsers();
        User GetUser(Guid id);
        User Create(User user);
        User Update(User user);
        User Delete(Guid id);
        User GetLogin(string userName, string hashPassword);
    }
}
