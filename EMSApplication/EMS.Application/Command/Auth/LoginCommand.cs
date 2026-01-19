using EMS.Application.Response;
using MediatR;

namespace EMS.Application.Command.Auth
{
    public record LoginCommand(string UserName, string Password) : IRequest<LoginResponse>;
}
