using System;
using System.Net;
using System.Threading.Tasks;

namespace BaGet.Core.Services
{

    public class CredentialsValidationService : ICredentialsValidationService
    {
        private readonly Func<ICredentials, string, Task<bool>> Callback;

        public CredentialsValidationService(Func<ICredentials, string, Task<bool>> callback)
        {
            Callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }

         Task<bool> ICredentialsValidationService.IsValid(ICredentials credentials, string httpMethod)
        {
            return Callback(credentials,httpMethod);
        }

    }



    public class UserService : ICredentialsValidationService
    { 
       // private readonly NetworkCredential[] Users;
        public UserService()
        {
           // Users = new NetworkCredential[] { new NetworkCredential("dummyUsername", "dummyPassword")};
        }
        public Task<bool> IsValid(ICredentials credential, string httpMethod = null)
        {
            if (credential == null)
            {
                throw new ArgumentNullException(nameof(credential));
            }
            return Task.FromResult(true);
        }

    }
}
