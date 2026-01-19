namespace EMS.Domain.Entities.Common
{
    public abstract class BaseEntity
    {
        public Guid Id { get; protected set; }

        public DateTime CreatedAt { get; protected set; } = DateTime.Now;
        public string? CreatedBy { get; protected set; }

        public DateTime? ModifiedAt { get; protected set; }
        public string? ModifiedBy { get; protected set; }

        public bool IsActive { get; protected set; }

        protected BaseEntity()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            IsActive = false;
        }

        public void MarkAsDeleted(string? deletedBy = null)
        {
            IsActive = true;
            ModifiedAt = DateTime.UtcNow;
            ModifiedBy = deletedBy;
        }

        public void UpdateAudit(string? modifiedBy = null)
        {
            ModifiedAt = DateTime.UtcNow;
            ModifiedBy = modifiedBy;
        }
    }

}
