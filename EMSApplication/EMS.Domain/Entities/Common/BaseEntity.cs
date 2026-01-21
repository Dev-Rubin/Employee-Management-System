using EMS.Domain.Common.Attributes;
using EMS.Domain.Entities.Common.Interface;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace EMS.Domain.Entities.Common
{
    public abstract class BaseEntity<TIdentity> : IBaseEntity<TIdentity>
    {
        public virtual TIdentity Id { get; protected set; }
        public virtual int? AddedByUserId { get; set; }
        public virtual DateTime? AddedDate { get; set; } = DateTime.Now;
        public virtual int? UpdatedByUserId { get; set; }
        public virtual DateTime? UpdatedDate { get; set; }
        private readonly List<ILegacyEvent> _events = new List<ILegacyEvent>();

        [Pure]
        [DebuggerNonUserCode]
        public virtual bool IsTransient()
        {
            return Equals(Id, default(TIdentity));
        }

        [Transient]
        public virtual IReadOnlyList<ILegacyEvent> Events
        {
            get { return _events; }
        }

        public virtual void ResetEvents()
        {
            _events.Clear();
        }
    }

}
