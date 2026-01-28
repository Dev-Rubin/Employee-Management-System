namespace EMS.Infrastructure.Persistence.UserContext
{
    public interface IUserContext
    {
        int? UserId { get; }
        string? UserName { get; }
        string? Role { get; }
        bool IsAuthenticated { get; }
    }
}
