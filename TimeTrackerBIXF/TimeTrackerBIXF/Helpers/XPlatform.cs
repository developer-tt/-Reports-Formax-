using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Xamarin.Essentials;

namespace TimeTrackerBIXF.Helpers
{
    public static class XPlatform
    {
        public static bool IsThereInternet => Connectivity.NetworkAccess == NetworkAccess.Internet;

        public static bool IsPasswordValid(this string Password)
        {

            if (String.IsNullOrEmpty(Password))
            {
                return false;
            }
            Match match = Regex.Match(Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$");
            if (match.Success)
            {
                return true;
            }
            return false;
        }

        public static bool IsEmailValid(this string Email)
        {
            return Regex.IsMatch(Email, @"^.+@[^\.].*\.[a-z]{2,}$");
        }

        public static string NormalizeResponse(this string Value)
        {
            return Value.Replace("\"", "");
        }

        public static string[] ReplaceAndSplit(this string Value)
        {
            return Value.Replace("\"", "").Split('|');
        }


    }
}
