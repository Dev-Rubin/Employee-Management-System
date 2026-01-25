using EMS.Application.IRepository;
using EMS.Domain.Entities;
using EMS.Infrastructure.Persistence.Service;
using System.Security.Cryptography;
using System.Text;

namespace EMS.Logic.Repository
{
    public class PasswordHasher : BasicCrudService<User, int>, IPasswordHasher
    {
        public bool Verify(string password, string hash, string salt)
        {
            using var hmac = new HMACSHA512(Convert.FromBase64String(salt));

            var computedHash = Convert.ToBase64String(
                hmac.ComputeHash(Encoding.UTF8.GetBytes(password))
            );

            return computedHash == hash;
        }
        public void CreatePasswordHash(string password, out string hash, out string salt)
        {
            using var hmac = new HMACSHA512();
            salt = Convert.ToBase64String(hmac.Key);
            hash = Convert.ToBase64String(
                hmac.ComputeHash(Encoding.UTF8.GetBytes(password))
            );
        }

    }
}
