namespace EMS.Domain.Entities.Common.Interface
{
    public interface IBaseEntity<out TIdentity> : IEntity
    {
        TIdentity Id { get; }
    }
}
