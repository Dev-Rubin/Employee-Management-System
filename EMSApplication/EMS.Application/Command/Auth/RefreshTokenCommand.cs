using EMS.Application.Response;
using MediatR;

namespace EMS.Application.Command.Auth
{
    public record RefreshTokenCommand(string RefreshToken) : IRequest<LoginResponse>;

}
