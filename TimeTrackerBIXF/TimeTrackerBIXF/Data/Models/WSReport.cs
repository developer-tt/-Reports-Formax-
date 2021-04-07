using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTrackerBIXF.Data.Models
{
    public class WSReport
    {
        public int UserID { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string GroupID { get; set; }
        public string ReportID { get; set; }
        public bool Parameter { get; set; }
        public string PTable { get; set; }
        public string PColumn { get; set; }
        public string PValue { get; set; }
        public string EmbeddedName { get; set; }

    }
}
