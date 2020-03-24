using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using System.Threading.Tasks;
using TimeTrackerBIXF.Utils;
using Xamarin.Essentials;

namespace TimeTrackerBIXF.DataServices
{
    public class PowerBIService
    {
        public static Task<TokenCredentials> GetAccessTokenSecretAsync()
        {
            return Task.Run(async () =>
            {
                var tenantSpecificURL = Constants.AuthorityUrl + await SecureStorage.GetAsync("TenantID");

                var authenticationContext = new AuthenticationContext(tenantSpecificURL);
                // Authentication using app credentials
                var credential = new ClientCredential(await SecureStorage.GetAsync("AppID"), await SecureStorage.GetAsync("AppSecret"));
                var authenticationResult = await authenticationContext.AcquireTokenAsync(Constants.ResourceUrl, credential);

                return new TokenCredentials(authenticationResult.AccessToken, "Bearer");
            });
        }
    }
}
