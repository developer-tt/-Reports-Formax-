using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TimeTrackerBIXF.Data.AuxModels;
using TimeTrackerBIXF.Helpers;
using TimeTrackerBIXF.Utils;

namespace TimeTrackerBIXF.DataServices
{
    public class UsersService
    {
        public async Task<Response> Login(string User, string Password)
        {
            Response response = new Response();

            if (XPlatform.IsThereInternet)
            {
                response = await WSMethods.Get(string.Format(Constants.Url_Users_Format, User, Password));
                return response;
            }
            else
            {
                response.Result = Result.NETWORK_UNAVAILABLE;
            }
            return response;

        }

        public async Task<Response> FormaxGetUserInfo(string Email)
        {
            Response response = new Response();

            if (XPlatform.IsThereInternet)
            {
                response = await WSMethods.Post(string.Format(Constants.Url_Formax_GetUserInfo, Email),null);
                return response;
            }
            else
            {
                response.Result = Result.NETWORK_UNAVAILABLE;
            }
            return response;

        }
    }
}
