using System.Net;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace BaGet.Core.Services
{
    public class UserService : IUserService
    {
        private readonly NetworkCredential[] Users;
        public UserService()
        {
            Users = new NetworkCredential[] { new NetworkCredential("dummyUser", "dummyPassword")};
        }
        public Task<bool> Authenticate(NetworkCredential credential)
        {
            if (credential == null)
            {
                throw new ArgumentNullException(nameof(credential));
            }
            var user = Users.SingleOrDefault(x => x.UserName == credential.UserName && x.Password == credential.Password);
            return Task.FromResult(user != null);
        }
    }
}
