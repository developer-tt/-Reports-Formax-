using Microsoft.PowerBI.Api.V2.Models;
using System;


namespace TimeTrackerBIXF.Data.AuxModels
{
    public class EmbedConfig
    {
        public string GroupID { get; set; }
        public string Id { get; set; }

        public string EmbedUrl { get; set; }

        public EmbedToken EmbedToken { get; set; }

        public int MinutesToExpiration
        {
            get
            {
                if (EmbedToken != null)
                {
                    var minutesToExpiration = EmbedToken.Expiration - DateTime.UtcNow;
                    return (int)minutesToExpiration?.TotalMinutes;
                }
                else
                {
                    return 0;
                }
            }
        }

        public bool? IsEffectiveIdentityRolesRequired { get; set; }

        public bool? IsEffectiveIdentityRequired { get; set; }

        public bool EnableRLS { get; set; }

        public string Username { get; set; }

        public string Roles { get; set; }

        public string ErrorMessage { get; internal set; }
        public string ReportName { get; set; }

        public bool Parameter { get; set; }
        public string PTable { get; set; }
        public string PColumn { get; set; }
        public string PValue { get; set; }
        public string Operator { get; set; }
    }
}
