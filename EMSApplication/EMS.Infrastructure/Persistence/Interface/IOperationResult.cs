namespace EMS.Infrastructure.Persistence.Interface
{
    public interface IOperationResult
    {
        string Message { get; }
        bool IsSuccessful { get; }
    }
}
