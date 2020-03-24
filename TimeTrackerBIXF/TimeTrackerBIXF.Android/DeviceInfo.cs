using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using TimeTrackerBIXF.Droid;
using TimeTrackerBIXF.Interfaces;
using static Android.Content.PM.PackageManager;

[assembly: Xamarin.Forms.Dependency(typeof(DeviceInfo))]
namespace TimeTrackerBIXF.Droid
{
    public class DeviceInfo : IDeviceInfo
    {
        public string GetDeviceName()
        {
            string manufacturer = Build.Manufacturer;
            string model = Build.Model;
            if (model.StartsWith(manufacturer))
            {
                return model.ToUpper();
            }
            return string.Format("{0} {1}", manufacturer.ToUpper(), model);
        }

        public string GetDeviceTypeID()
        {
            
            bool xlarge = ((Application.Context.Resources.Configuration.ScreenLayout & ScreenLayout.SizeMask) == ScreenLayout.SizeXlarge);
            bool large = ((Application.Context.Resources.Configuration.ScreenLayout & ScreenLayout.SizeMask) == ScreenLayout.SizeLarge);
            return (xlarge || large) ? "4" : "3";
        }

        public string GetAppVersion()
        {
            PackageManager manager = Application.Context.PackageManager;
            PackageInfo info = null;
            try
            {
                info = manager.GetPackageInfo(Application.Context.PackageName, 0);
                return info.VersionName;
            }
            catch (NameNotFoundException ex)
            {

                return string.Empty;
            }
        }

        public string GetOSVersion()
        {
            return Build.VERSION.Release;
        }
    }
}