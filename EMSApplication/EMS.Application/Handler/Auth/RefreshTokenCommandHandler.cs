using EMS.Application.Command.Auth;
using EMS.Application.IRepository;
using EMS.Application.Response;
using MediatR;

namespace EMS.Application.Handler.Auth
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, LoginResponse>
    {
        private readonly IUserRepository _userRepo;
        private readonly IJwtTokenGenerator _tokenGenerator;
        public RefreshTokenCommandHandler(IUserRepository userRepo, IJwtTokenGenerator tokenGenerator)
        {
            _userRepo = userRepo;
            _tokenGenerator = tokenGenerator;
        }
        public async Task<LoginResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepo.GetByRefreshTokenAsync(request.RefreshToken);

            if (user == null)
                throw new UnauthorizedAccessException();

            var token = await _tokenGenerator.Generate(user);

            return new LoginResponse(
                token.Token,
                token.ExpiresAt
            );
        }
    }

}
