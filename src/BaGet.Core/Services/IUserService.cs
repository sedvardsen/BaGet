using System.Net;
using System.Threading.Tasks;

namespace BaGet.Core.Services
{
    public interface IUserService
    {
        Task<bool> Authenticate(NetworkCredential credential);
    }
}
