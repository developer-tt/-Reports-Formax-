using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace TimeTrackerBIXF.DataServices
{
    public class AadService
    {
        private static readonly string m_authorityUrl = "https://login.microsoftonline.com/organizations/";
        private static readonly string[] m_scope = "https://analysis.windows.net/powerbi/api/.default".Split(';');

        /// <summary>
        /// Get Access token
        /// </summary>
        /// <returns>Access token</returns>
        public static async Task<string> GetAccessToken()
        {
            AuthenticationResult authenticationResult = null;
      
         
            
                // For app only authentication, we need the specific tenant id in the authority url
                var tenantSpecificURL = m_authorityUrl.Replace("organizations", "f47e4ba5-8a0e-4eaa-ac0b-5f12dee07155");

                IConfidentialClientApplication clientApp = ConfidentialClientApplicationBuilder
                                                                                .Create("c771213b-070e-444b-b5d5-96abd042d923")
                                                                                .WithClientSecret("kDO8WF5GuJo3u5qZKray_PZUE16M~b.fP-")
                                                                                .WithAuthority(tenantSpecificURL)
                                                                                .Build();

                authenticationResult = await clientApp.AcquireTokenForClient(m_scope).ExecuteAsync();
            

            return authenticationResult.AccessToken;
        }
    }
}
