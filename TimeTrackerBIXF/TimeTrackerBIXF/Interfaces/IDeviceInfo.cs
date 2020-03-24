using System;
using System.Collections.Generic;
using System.Text;

namespace TimeTrackerBIXF.Interfaces
{
    public interface IDeviceInfo
    {
        string GetDeviceTypeID();
        string GetDeviceName();
        string GetAppVersion();
        string GetOSVersion();
    }
}
