using System.Collections.Generic;

namespace TimeTrackerBIXF.Data.Models
{
    public class WSFormaxUserInfo
    {
        public bool Success { get; set; }
        public object Error { get; set; }
        public int UserID { get; set; }
        public List<int> LevelIDs { get; set; }
       
    }
}
