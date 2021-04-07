using SQLite;
using System;
using TimeTrackerBIXF.Business;
using TimeTrackerBIXF.Data.AuxModels;
using TimeTrackerBIXF.Data.Models;
using TimeTrackerBIXF.Interfaces;
using TimeTrackerBIXF.Views;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TimeTrackerBIXF
{
    public partial class App : Application
    {
        private readonly SQLiteConnection dbConnection;
        public static UsersB UsersB { get; set; }
        public static EmbedTokenB EmbedTokenB { get; set; }
        public static tblUsersDTO CurrentUser { get { return UsersB.Get(); } }
        private static string DeviceName { get; set; }
        private static string DeviceTypeID { get; set; }
        private static string AppVersion { get; set; }
        private static string OSVersion { get; set; }
        public App()
        {
            InitializeComponent();
            VersionTracking.Track();

            dbConnection = DependencyService.Get<ISQLite>().GetConnection();

            DeviceName = DependencyService.Get<IDeviceInfo>().GetDeviceName();
            DeviceTypeID = DependencyService.Get<IDeviceInfo>().GetDeviceTypeID();
            AppVersion = DependencyService.Get<IDeviceInfo>().GetAppVersion();
            OSVersion = DependencyService.Get<IDeviceInfo>().GetOSVersion();

            UsersB = new UsersB(dbConnection);
            EmbedTokenB = new EmbedTokenB(dbConnection);

            SecureStorage.SetAsync("AppID", "6f965ae2-66e4-42fb-9441-24223cd5bbee");
            SecureStorage.SetAsync("AppSecret", "rlt/vOo031wEm+KcfFi+2lwbWdvc8DHq7YUF5pot0CE=");
            SecureStorage.SetAsync("TenantID", "f47e4ba5-8a0e-4eaa-ac0b-5f12dee07155");


            if (CurrentUser != null)
            {
                if (CurrentUser.DeviceID == 0)
                {
                    MainPage = new Login();
                }
                else
                {
                    if (CurrentUser.DeviceStatus == DeviceStatus.Pending)
                    {
                        MainPage = new Confirmation();
                    }
                    else if (CurrentUser.DeviceStatus == DeviceStatus.Approved)
                    {
                        MainPage = new NavigationPage(new Reports());
                    }
                    else if (CurrentUser.DeviceStatus == DeviceStatus.Rejected)
                    {
                        MainPage = new Login();
                    }
                }
            }
            else
            {
                MainPage = new Login();
            }


        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        public static string GetDeviceName()
        {
            return DeviceName;
        }

        public static string GetDeviceTypeID()
        {
            return DeviceTypeID;
        }

        public static string GetAppVersion()
        {
            return AppVersion;
        }

        public static string GetOSVersion()
        {
            return OSVersion;
        }
    }
}
