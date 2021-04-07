using TimeTrackerBIXF.Interfaces;
using UIKit;
using Xamarin.Essentials;

[assembly: Xamarin.Forms.Dependency(typeof(TimeTrackerBIXF.iOS.DeviceInfo))]
namespace TimeTrackerBIXF.iOS
{
    public class DeviceInfo : IDeviceInfo
    {
        public string GetAppVersion()
        {
            return VersionTracking.CurrentVersion;
        }

        public string GetDeviceName()
        {
            return Xamarin.Essentials.DeviceInfo.Name;
        }

        public string GetDeviceTypeID()
        {
            string DeviceModel = UIDevice.CurrentDevice.Model.Split(' ')[0];

            return DeviceModel == "iPhone" ? "1" : "2";
        }

        public string GetOSVersion()
        {
            return Xamarin.Essentials.DeviceInfo.VersionString;
        }
    }
}