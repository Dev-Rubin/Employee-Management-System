using EMS.Domain.Entities;

namespace EMS.Application.Repository
{
    public interface IUserRepository
    {
        Task<User?> GetByUserNameAsync(string userName);
        Task<User?> GetByRefreshTokenAsync(string token);
        Task AddAsync(User user);
    }
}
