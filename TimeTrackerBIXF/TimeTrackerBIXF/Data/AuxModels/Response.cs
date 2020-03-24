using System;
using System.Collections.Generic;
using System.Text;

namespace TimeTrackerBIXF.Data.AuxModels
{
    public enum Result
    {
        OK,
        ERROR,
        BAD_REQUEST,
        NOT_FOUND,
        INVALID_PASSWORD,
        EXCEPTION,
        BLOCKED_USER,
        NOT_REGISTERED,
        ALREADY_EXISTS,
        NOT_AUTHORIZED,
        ERROR_GETTING_DATA,
        SERVICE_EXCEPTION,
        NETWORK_UNAVAILABLE
    }
    public class Response
    {
        public Result Result { get; set; }
        public string Data { get; set; }
    }
}
