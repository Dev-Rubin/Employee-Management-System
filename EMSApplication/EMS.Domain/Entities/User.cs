using EMS.Domain.Entities.Common;
using EMS.Domain.Entities.Enums;

namespace EMS.Domain.Entities
{
    public class User : AuditableEntity
    {
        public string UserName { get; private set; } = default!;
        public string Email { get; private set; } = default!;
        public string PasswordHash { get; private set; } = default!;
        public string PasswordSalt { get; private set; } = default!;
        public UserRole Role { get; private set; }
        public bool IsActive { get; private set; }

        private User() { } 

        public User(
            string userName,
            string email,
            string passwordHash,
            string passwordSalt,
            UserRole role)
        {
            UserName = userName;
            Email = email;
            PasswordHash = passwordHash;
            PasswordSalt = passwordSalt;
            Role = role;
            IsActive = true;
        }

        public void Deactivate(string? modifiedBy = null)
        {
            IsActive = false;
            UpdateAudit(modifiedBy);
        }
    }

}
