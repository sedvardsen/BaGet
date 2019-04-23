using System.Threading.Tasks;
using System.Net;

namespace BaGet.Core.Authentication
{
    public interface IAuthenticationService
    {
        Task<bool> AuthenticateAsync(string apiKey);
    }
}
