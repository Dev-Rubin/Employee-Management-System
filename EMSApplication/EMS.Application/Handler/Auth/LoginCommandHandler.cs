using EMS.Application.Command.Auth;
using EMS.Application.IRepository;
using EMS.Application.Response;
using MediatR;
namespace EMS.Application.Handler.Auth
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
    {
        private readonly IUserRepository _userRepo;
        private readonly IPasswordHasher _hasher;
        private readonly IJwtTokenGenerator _tokenGenerator;
        public LoginCommandHandler(IUserRepository userRepo, IPasswordHasher hasher, IJwtTokenGenerator tokenGenerator)
        {
            _userRepo = userRepo;
            _hasher = hasher;
            _tokenGenerator = tokenGenerator;
        }

        public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepo.GetByUserNameAsync(request.UserName);

            if (user == null || !user.IsActive)
                throw new UnauthorizedAccessException("Invalid credentials");

            var isValid = _hasher.Verify(
                request.Password,
                user.PasswordHash,
                user.PasswordSalt
            );

            if (!isValid)
                throw new UnauthorizedAccessException("Invalid credentials");

            var token = await _tokenGenerator.Generate(user);

            return new LoginResponse(
                token.Token,
                token.ExpiresAt
            );
        }
    }
}

