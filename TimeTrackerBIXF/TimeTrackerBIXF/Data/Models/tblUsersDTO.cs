using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTrackerBIXF.Data.Models
{
    public class tblUsersDTO
    {
        [PrimaryKey]
        public int UserID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Code { get; set; }
        public int DeviceID { get; set; }
        public string DeviceStatus { get; set; }
        public string Levels { get; set; }

        [Ignore]
        public List<int> LevelIDs
        {
            get
            {
                if (!string.IsNullOrEmpty(Levels))
                {
                    return Levels.Split(',').Select(a=> int.Parse(a)).ToList();
                }
                else
                {
                    return null;
                }
            }

        }
    }
}
