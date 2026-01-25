using EMS.Domain.Entities;

namespace EMS.Application.IRepository
{
    public interface IJwtTokenGenerator
    {
        Task<RefreshToken> Generate(User user);
    }
}
