
using EMS.Domain.Entities.Enums;
using MediatR;

namespace EMS.Application.Command.Auth
{
    public record RegisterUserCommand(string UserName, string Email, string Password, UserRole userRole = UserRole.User) : IRequest<Guid>;
}
