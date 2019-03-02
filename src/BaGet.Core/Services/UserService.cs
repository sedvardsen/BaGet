using System;
using System.Net;
using System.Threading.Tasks;

namespace BaGet.Core.Services
{

    public class CredentialsValidationService : ICredentialsValidationService
    {
        private readonly Func<ICredentials, Task<bool>> Callback;

        public CredentialsValidationService(Func<ICredentials, Task<bool>> callback)
        {
            Callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }

         Task<bool> ICredentialsValidationService.IsValid(ICredentials credentials)
        {
            return Callback(credentials);
        }

    }



    public class UserService : ICredentialsValidationService
    { 
       // private readonly NetworkCredential[] Users;
        public UserService()
        {
           // Users = new NetworkCredential[] { new NetworkCredential("dummyUsername", "dummyPassword")};
        }
        public Task<bool> IsValid(ICredentials credential)
        {
            if (credential == null)
            {
                throw new ArgumentNullException(nameof(credential));
            }
            return Task.FromResult(true);
        }

    }
}
