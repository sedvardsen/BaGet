using System.Threading.Tasks;
using System.Net;

namespace BaGet.Core.Services
{
    public interface IAuthenticationService
    {
        Task<bool> AuthenticateAsync(string apiKey);
    }
}
