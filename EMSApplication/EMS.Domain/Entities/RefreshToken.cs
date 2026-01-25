using EMS.Domain.Entities.Common;

namespace EMS.Domain.Entities
{
    public class RefreshToken : BaseEntity<int>
    {
        public string Token { get; private set; } = default!;
        public int UserId { get; private set; }
        public DateTime ExpiresAt { get; private set; }
        public bool IsRevoked { get; private set; }
        public User User { get; set; }

        private RefreshToken() { }

        public RefreshToken(int userId, string token, DateTime expires)
        {
            Token = token;
            UserId = userId;
            ExpiresAt = expires;
        }

        public void Revoke(string? modifiedBy = null)
        {
            IsRevoked = true;
        }

        public bool IsValid() => !IsRevoked && DateTime.UtcNow <= ExpiresAt;
    }

}
