using EMS.Domain.Entities;

namespace EMS.Application.IRepository
{
    public interface IJwtTokenGenerator
    {
        string Generate(User user);
    }
}
