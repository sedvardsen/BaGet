using System.Threading.Tasks;
using System.Net;

namespace BaGet.Core.Services
{
    public interface ICredentialsValidationService
    {
        Task<bool> IsValid(ICredentials credentials, string httpMethod = null);
    }
}
