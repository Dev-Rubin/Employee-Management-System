namespace EMS.Application.Repository
{
    public interface IPasswordHasher
    {
        bool Verify(string password, string hash, string salt);
        void CreatePasswordHash(string password, out string hash, out string salt);
    }
}
