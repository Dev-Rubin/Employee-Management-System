using EMS.Domain.Entities;

namespace EMS.Application.Repository
{
    public interface IJwtTokenGenerator
    {
        string Generate(User user);
    }
}
