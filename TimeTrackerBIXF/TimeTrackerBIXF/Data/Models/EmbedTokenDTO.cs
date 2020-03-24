using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace TimeTrackerBIXF.Data.Models
{
    public class EmbedTokenDTO
    {
        [PrimaryKey]
        [AutoIncrement]
        public int TokenAutoID { get; set; }
        public string Token { get; set; }

        public string TokenId { get; set; }

        public DateTime? Expiration { get; set; }

        public string GroupID { get; set; }
        public string Id { get; set; }

        public int MinutesToExpiration
        {
            get
            {


                if (Expiration != null)
                {
                    if (Expiration.HasValue)
                    {
                        var minutesToExpiration = Expiration - DateTime.UtcNow;
                        return (int)minutesToExpiration?.TotalMinutes;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}
