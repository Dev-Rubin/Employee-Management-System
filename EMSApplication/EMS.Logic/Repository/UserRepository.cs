using EMS.Application.IRepository;
using EMS.Domain.Entities;
using EMS.Infrastructure.Persistence;
using EMS.Infrastructure.Persistence.Interface;
using EMS.Infrastructure.Persistence.Service;
using Microsoft.EntityFrameworkCore;

namespace EMS.Logic.Repository
{
    public class UserRepository : BasicCrudService<User, int >, IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByUserNameAsync(string userName)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x => x.UserName == userName);
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task<User?> GetByRefreshTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .Where(x => x.Token == token && !x.IsRevoked)
                .Select(x => x.User)
                .FirstOrDefaultAsync();
        }
    }
}
