using EMS.Application.IRepository;
using EMS.Domain.Entities;
using EMS.Infrastructure.Persistence;
using EMS.Infrastructure.Persistence.Interface;
using EMS.Infrastructure.Persistence.Service;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace EMS.Logic.Repository
{
    public class UserRepository : BasicCrudService<User, int >, IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context, IUnitOfWork unitOfWork, IRepository repository, IQueries queries) : base(unitOfWork, repository, queries)
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

        public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
        {

            var hash = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken))
            );

            return await _context.RefreshTokens
                .Where(x => x.TokenHash == hash && !x.IsValid())
                .Select(x => x.User)
                .FirstOrDefaultAsync();
        }

    }
}
