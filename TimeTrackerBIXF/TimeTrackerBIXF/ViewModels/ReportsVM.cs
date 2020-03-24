using Microsoft.PowerBI.Api.V2;
using Microsoft.PowerBI.Api.V2.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using TimeTrackerBIXF.Data.AuxModels;
using TimeTrackerBIXF.Data.Models;
using TimeTrackerBIXF.DataServices;
using TimeTrackerBIXF.Helpers;
using TimeTrackerBIXF.Utils;
using TimeTrackerBIXF.Views;
using Xamarin.Forms;

namespace TimeTrackerBIXF.ViewModels
{
    public class ReportsVM : BaseVM
    {
        public ICommand RefreshCommand { get; }

        public ICommand GoToSettingsCommand => new Command(GoToSettings);
        public ICommand EnterPressedCommand => new Command(EnterPressed);

        public WSReport ReportSelected { get; set; }

        public ObservableCollection<WSReport> Items { get; set; }
        public List<WSReport> PrefilteredItems { get; set; }

        PowerBIService PowerBiService;
        ReportsService ReportsService;

        public bool IsRefreshing
        {
            get; set;
        }

        string GroupID = string.Empty;

        public string Name { get; set; }
        public string SearchText { get; set; }

        public ReportsVM()
        {
            PrefilteredItems = new List<WSReport>();
            Items = new ObservableCollection<WSReport>();
            RefreshCommand = new Command(ExecuteRefreshCommand);

            Name = string.Format("Hola, {0}!", App.CurrentUser.Name);

            ReportsService = new ReportsService();
            PowerBiService = new PowerBIService();

            GetGroupID();

        }

        private void EnterPressed()
        {
            if (string.IsNullOrEmpty(SearchText) || string.IsNullOrWhiteSpace(SearchText))
            {
                Items = new ObservableCollection<WSReport>(PrefilteredItems); 
            }
            else
            {
                var NewItems = PrefilteredItems.Where(a => a.Name.ToLower().Contains(SearchText.ToLower())).ToList();
                Items.Clear();
                Items = new ObservableCollection<WSReport>(NewItems);
            }
        }

        public void OnReportSelectedChanged()
        {
            if (ReportSelected != null)
            {
                App.Current.MainPage.Navigation.PushAsync(new ReportViewer(new EmbedConfig() { GroupID = ReportSelected.GroupID, Id = ReportSelected.ReportID, EmbedUrl = ReportSelected.Url, ReportName = ReportSelected.Name }), true);
                ReportSelected = null;
            }
        }

        private async void GetGroupID()
        {
            IsRefreshing = true;
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Alerts.ShowLoadingPageAsync("Obteniendo reportes");

            });

            Response response = await ReportsService.GetReports(App.CurrentUser.UserID);
            if (response.Result != Result.NETWORK_UNAVAILABLE)
            {

                if (response.Result == Result.ERROR_GETTING_DATA)
                {
                    await Alerts.HideLoadingPageAsync();
                    Alerts.ShowAlert(string.Empty, "Error al obtener datos del servidor");
                }
                else if (response.Result == Result.SERVICE_EXCEPTION)
                {
                    await Alerts.HideLoadingPageAsync();
                    Alerts.ShowAlert(string.Empty, "Error al obtener datos del dispositivo (SE)");
                }
                else if (response.Result == Result.OK)
                {
                    if (response.Data != null)
                    {
                        var Items = JsonConvert.DeserializeObject<List<WSReport>>(response.Data);
                        GroupID = Items.Where(a => !string.IsNullOrEmpty(a.GroupID)).Select(b => b.GroupID).FirstOrDefault();

                        await Alerts.HideLoadingPageAsync();

                        if (!string.IsNullOrEmpty(GroupID))
                        {
                            IsRefreshing = false;
                            GetPowerBIReports();
                        }
                    }
                    else
                    {
                        await Alerts.HideLoadingPageAsync();
                        Alerts.ShowAlert(string.Empty, "Error al parsear los datos");
                    }
                }


            }
            else
            {
                await Alerts.HideLoadingPageAsync();
                Alerts.ShowNetworkError();
            }




            IsRefreshing = false;
        }

        private async void GetPowerBIReports()
        {
            IsRefreshing = true;
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Alerts.ShowLoadingPageAsync("Obteniendo reportes");

            });


            if (XPlatform.IsThereInternet)
            {
                try
                {
                    var tokenCredentials = await PowerBIService.GetAccessTokenSecretAsync();

                    // Create a Power BI Client object. it's used to call Power BI APIs.
                    using (var client = new PowerBIClient(new Uri(Constants.ApiUrl), tokenCredentials))
                    {
                        ODataResponseListReport ReportsInGroup = client.Reports.GetReportsInGroup(GroupID);

                        Items.Clear();
                        PrefilteredItems.Clear();

                        if (ReportsInGroup != null)
                        {
                            foreach (Report Report in ReportsInGroup.Value)
                            {
                                Items.Add(new WSReport() { Name = Report.Name, Url = Report.EmbedUrl, ReportID = Report.Id, GroupID = GroupID });
                                PrefilteredItems.Add(new WSReport() { Name = Report.Name, Url = Report.EmbedUrl, ReportID = Report.Id, GroupID = GroupID });
                            }
                        }

                        await Alerts.HideLoadingPageAsync();

                    }
                }
                catch (Exception ex)
                {
                    await Alerts.HideLoadingPageAsync();
                    Alerts.ShowAlert(string.Empty, "Ocurrio un error al obtener los reportes de PowerBI.");
                    string Exception = ex.Message;
                }
            }
            else
            {
                await Alerts.HideLoadingPageAsync();
                Alerts.ShowNetworkError();
            }


            IsRefreshing = false;
        }

        void ExecuteRefreshCommand()
        {
            if (IsRefreshing)
                return;

            IsRefreshing = true;

            if (string.IsNullOrEmpty(GroupID))
            {
                GetGroupID();
            }
            else
            {
                GetPowerBIReports();
            }

            IsRefreshing = false;
        }


        private void GoToSettings()
        {
            App.Current.MainPage.Navigation.PushAsync(new Settings(), true);
        }


    }
}
