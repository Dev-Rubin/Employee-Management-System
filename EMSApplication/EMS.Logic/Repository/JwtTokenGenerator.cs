using EMS.Application.IRepository;
using EMS.Domain.Entities;
using EMS.Infrastructure.Persistence.Interface;
using EMS.Infrastructure.Persistence.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace EMS.Logic.Repository
{


    public class JwtTokenGenerator : BasicCrudService<RefreshToken, int>, IJwtTokenGenerator
    {
        private readonly IConfiguration _config;

        public JwtTokenGenerator(IAppDbContext appDbContext, IConfiguration config, IUnitOfWork unitOfWork, IRepository repository, IQueries queries) : base(unitOfWork, repository, queries)
        {
            _config = config;
        }

        public async Task<RefreshToken> Generate(User user)
        {
            var keyString = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key missing");

            if (Encoding.UTF8.GetByteCount(keyString) < 32)
                throw new InvalidOperationException("JWT Key must be at least 32 bytes");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };
            int expireHours = _config.GetValue<int?>("Jwt:ExpireHours") ?? 1;
            DateTime expires = DateTime.UtcNow.AddHours(expireHours);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            string generatedToken= new JwtSecurityTokenHandler().WriteToken(token);

            var refreshToken = new RefreshToken(user.Id, generatedToken, expires);

            var result =  await Transact.ExecuteWithTransactionAsync(
                () =>
                {
                    Repository.SaveUpdate(refreshToken);
                }, "Token generated successfully.", "Failed to generate token."
            ).ConfigureAwait(false);

            if (!result.Result.IsSuccessful)
            {
                throw new Exception(result.Result.Message);
            }

            return refreshToken;
        }
    }

}
