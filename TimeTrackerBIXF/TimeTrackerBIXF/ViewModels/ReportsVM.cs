
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public List<WSReport> APIAvailableItems { get; set; }

        PowerBIService PowerBiService;
        ReportsService ReportsService;


        public bool IsRefreshing
        {
            get; set;
        }


        public string Name { get; set; }
        public string SearchText { get; set; }
        UsersService UsersService = new UsersService();

        List<string> GroupIDs { get; set; }

        public ReportsVM()
        {
            PrefilteredItems = new List<WSReport>();
            Items = new ObservableCollection<WSReport>();
            RefreshCommand = new Command(ExecuteRefreshCommand);

            Name = string.Format("Hola, {0}!", App.CurrentUser.Name);

            ReportsService = new ReportsService();
            PowerBiService = new PowerBIService();

            GetTTReports();
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

        public async void OnReportSelectedChanged()
        {
            if (ReportSelected != null)
            {

                Report report = await GetReportByGroupAndName(ReportSelected.GroupID, ReportSelected.EmbeddedName);

                if (report == null)
                {
                    Alerts.ShowAlert("Alerta", "No se encontro referencia en PowerBI de este reporte.");
                }
                else
                {
                    await App.Current.MainPage.Navigation.PushAsync(new ReportViewer(new EmbedConfig() { GroupID = ReportSelected.GroupID, Id = report.Id.ToString(), EmbedUrl = report.EmbedUrl, ReportName = ReportSelected.Name, Parameter = ReportSelected.Parameter, PColumn = ReportSelected.PColumn, PTable = ReportSelected.PTable, PValue = ReportSelected.PValue }), true);
                }

                ReportSelected = null;
            }
        }

        private async void GetTTReports()
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
                        APIAvailableItems = JsonConvert.DeserializeObject<List<WSReport>>(response.Data);
                        Items = new ObservableCollection<WSReport>(APIAvailableItems);
                        PrefilteredItems = APIAvailableItems;

                        GroupIDs = APIAvailableItems.Where(a => !string.IsNullOrEmpty(a.GroupID)).Select(b => b.GroupID).ToList();


                        await Alerts.HideLoadingPageAsync();

                        //if (GroupIDs != null && GroupIDs.Count > 0)
                        //{
                        //    IsRefreshing = false;
                        //    //GetPowerBIReports();
                        //}
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

        private async Task<Report> GetReportByGroupAndName(string GroupID, string Name)
        {
            try
            {
                if (string.IsNullOrEmpty(GroupID) || string.IsNullOrEmpty(Name))
                {
                    return null;
                }
                var tokenCredentials = await PowerBIService.GetAccessTokenSecretAsync();

                // Create a Power BI Client object. it's used to call Power BI APIs.
                using (var client = new PowerBIClient(new Uri(Constants.ApiUrl), tokenCredentials))
                {

                    Microsoft.PowerBI.Api.Models.Reports ReportsInGroup = client.Reports.GetReportsInGroup(Guid.Parse(GroupID));

                    if (ReportsInGroup != null)
                    {
                        Report found = ReportsInGroup.Value.Where(a => a.Name.Equals(Name)).FirstOrDefault();
                        return ReportsInGroup.Value.Where(a => a.Name.Equals(Name)).FirstOrDefault();
                        //foreach (Report Report in ReportsInGroup.Value)
                        //{
                        //    WSReport rpt = APIAvailableItems.Where(a => a.Name.Equals(Report.Name)).FirstOrDefault();

                        //    if (rpt != null)
                        //    {
                        //        reports.Add(new WSReport() { Name = Report.Name, Url = Report.EmbedUrl, ReportID = Report.Id, GroupID = GroupID, Parameter = rpt.Parameter, PColumn = rpt.PColumn, PTable = rpt.PTable, PValue = rpt.PValue });
                        //    }
                        //}
                    }
                }
            }
            catch (Microsoft.IdentityModel.Clients.ActiveDirectory.AdalServiceException)
            {

            }
            catch (Exception ex)
            {

            }

            return null;
        }

        void ExecuteRefreshCommand()
        {
            if (IsRefreshing)
                return;

            IsRefreshing = true;


            if (GroupIDs != null && GroupIDs.Count > 0)
            {
                GetTTReports();
            }
            else
            {
                //GetPowerBIReports();
            }

            IsRefreshing = false;
        }


        private void GoToSettings()
        {
            App.Current.MainPage.Navigation.PushAsync(new Settings(), true);
        }


    }
}
