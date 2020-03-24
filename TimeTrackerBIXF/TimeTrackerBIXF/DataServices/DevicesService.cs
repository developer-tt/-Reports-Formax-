using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TimeTrackerBIXF.Data.AuxModels;
using TimeTrackerBIXF.Data.Models;
using TimeTrackerBIXF.Helpers;
using TimeTrackerBIXF.Utils;

namespace TimeTrackerBIXF.DataServices
{
    public class DevicesService
    {
        public async Task<Response> RegisterDevice(WSDevice WSDevice)
        {
            Response response = new Response();

            if (XPlatform.IsThereInternet)
            {
                response = await WSMethods.Post(Constants.Url_Devices, WSDevice);
                return response;
            }
            else
            {
                response.Result = Result.NETWORK_UNAVAILABLE;
            }
            return response;

        }

        public async Task<Response> GetDeviceStatus(int DeviceID)
        {
            Response response = new Response();

            if (XPlatform.IsThereInternet)
            {
                response = await WSMethods.Get(Constants.Url_Devices + DeviceID);
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
