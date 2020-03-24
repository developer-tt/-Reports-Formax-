using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTrackerBIXF.Data.Models
{
    public class WSDevice
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string DeviceTypeID { get; set; }
        public string AppVersion { get; set; }
        public string Model { get; set; }
        public string OSVersion { get; set; }
    }
}
