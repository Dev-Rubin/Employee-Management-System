using EMS.Domain.Entities.Common;
using System.Security.Cryptography;
using System.Text;

namespace EMS.Domain.Entities
{
    public class RefreshToken : BaseEntity<int>
    {
        public string Token { get; private set; } = default!;        
        public string TokenHash { get; private set; } = default!;  

        public int UserId { get; private set; }
        public DateTime ExpiresAt { get; private set; }
        public bool IsRevoked { get; private set; }

        public User User { get; set; } = default!;

        private RefreshToken() { }

        public RefreshToken(int userId, string token, DateTime expiresAt)
        {
            UserId = userId;
            Token = token;
            TokenHash = ComputeHash(token);
            ExpiresAt = expiresAt;
        }

        public void Revoke()
        {
            IsRevoked = true;
        }

        public bool IsValid() => !IsRevoked && DateTime.UtcNow <= ExpiresAt;

        private static string ComputeHash(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(bytes); 
        }
    }


}
