using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TimeTrackerBIXF.Data.AuxModels;
using TimeTrackerBIXF.Helpers;
using TimeTrackerBIXF.Utils;

namespace TimeTrackerBIXF.DataServices
{
    public class ReportsService
    {
        public async Task<Response> GetReports(int UserID)
        {
            Response response = new Response();

            if (XPlatform.IsThereInternet)
            {
                response = await WSMethods.Get(string.Format(Constants.Url_Reports_Format, UserID));
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
