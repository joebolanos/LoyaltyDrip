using System.Threading.Tasks;

namespace Core.Shared.Security
{
    public interface IAuthenticationService
    {
        Task<string> GenerateJWT(string username, string password);
    }
}
