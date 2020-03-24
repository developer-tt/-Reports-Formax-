using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using TimeTrackerBIXF.Interfaces;
using TimeTrackerBIXF.iOS;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(DeviceInfo))]
namespace TimeTrackerBIXF.iOS
{
    public class DeviceInfo : IDeviceInfo
    {
        public string GetAppVersion()
        {
            throw new NotImplementedException();
        }

        public string GetDeviceName()
        {
            throw new NotImplementedException();
        }

        public string GetDeviceTypeID()
        {
            string DeviceModel = UIDevice.CurrentDevice.Model.Split(' ')[0];

            return DeviceModel == "iPhone" ? "1" : "2";
        }

        public string GetOSVersion()
        {
            throw new NotImplementedException();
        }
    }
}