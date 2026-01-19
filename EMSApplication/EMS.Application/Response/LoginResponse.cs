namespace EMS.Application.Response
{
    public record LoginResponse(string AccessToken, DateTime ExpiresAt);
}
