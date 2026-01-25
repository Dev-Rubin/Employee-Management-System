using EMS.Application.Command.Auth;
using EMS.Application.IRepository;
using EMS.Domain.Entities;
using MediatR;

namespace EMS.Application.Handler.Auth
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, int>
    {
        private readonly IUserRepository _repo;
        private readonly IPasswordHasher _hasher;
        public RegisterUserCommandHandler(IUserRepository repo, IPasswordHasher hasher)
        {
            _repo = repo;
            _hasher = hasher;
        }

        public async Task<int> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            _hasher.CreatePasswordHash( request.Password, out string hash, out string salt);

            var user = new User(request.UserName, request.Email, hash, salt, request.userRole);

            await _repo.AddAsync(user);
            return user.Id;
        }
    }

}
