namespace EMS.Domain.Entities.Common.Interface
{
    public interface IEntity
    {
        bool IsTransient();

        IReadOnlyList<ILegacyEvent> Events { get; }

        void ResetEvents();
    }
}
