
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TimeTrackerBIXF.Data.AuxModels;
using TimeTrackerBIXF.DataServices;
using TimeTrackerBIXF.Helpers;
using TimeTrackerBIXF.Interfaces;
using TimeTrackerBIXF.Utils;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TimeTrackerBIXF.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ReportViewer : ContentPage
    {
        EmbedConfig EmbedConfig;
        PowerBIService PowerBIService = new PowerBIService();
        private static TokenCredentials tokenCredentials = null;


        private Timer Timer_StatusCheck;
        private Timer Timer_DocLoadedCheck;

        bool IsChecking = false;
        bool IsReadyChecking = false;
        bool Configured = false;
        bool Configuring = false;

        List<string> ReportPages = new List<string>();
        List<string> ReportPagesIds = new List<string>();

        public ReportViewer(EmbedConfig EConfig)
        {
            InitializeComponent();


            EmbedConfig = EConfig;


            ReportName.Text = EmbedConfig.ReportName;

            pickerReportPages.ItemsSource = ReportPages;

            _webView.Source = null;

            _webView.Source = LoadHTMLFileFromResource();

            StartDocLoadedTimer();
        }

        private async void GenerateEmbedToken()
        {
            try
            {
                tokenCredentials = await PowerBIService.GetAccessTokenSecretAsync();

                using (var client = new PowerBIClient(new Uri(Constants.ApiUrl), tokenCredentials))
                {

                    var TokenCached = App.EmbedTokenB.GetByGroupIDAndReportID(EmbedConfig.GroupID, EmbedConfig.Id);

                    if (TokenCached == null)
                    {
                        //IList<Guid> ds = new List<Guid>();
                        //ds.Add(Guid.Parse(EmbedConfig.GroupID));
                        EmbedToken tokenResponse =client.Reports.GenerateToken(Guid.Parse( EmbedConfig.GroupID),Guid.Parse( EmbedConfig.Id), new GenerateTokenRequest(accessLevel: "view"));

                        EmbedConfig.EmbedToken = tokenResponse;

                        App.EmbedTokenB.Create(new Data.Models.EmbedTokenDTO() { Expiration = tokenResponse.Expiration, Token = tokenResponse.Token, GroupID = EmbedConfig.GroupID, Id = EmbedConfig.Id, TokenId = tokenResponse.TokenId.ToString() });


                        ConfigureReportViewer();
                    }
                    else
                    {
                        EmbedConfig.EmbedToken = new EmbedToken(TokenCached.Token, Guid.Parse(TokenCached.TokenId), TokenCached.Expiration.Value);

                        ConfigureReportViewer();
                    }
                }
            }
            catch (HttpOperationException ex)
            {
                var content = ex.Response.Content;
                Alerts.ShowAlert(string.Empty, "Error al mostrar el reporte.");
            }
        }


        public static EmbedToken GetEmbedToken(PowerBIClient client, Guid reportId, IList<Guid> datasetIds, [Optional] Guid targetWorkspaceId)
        {
            using (var pbiClient = client)
            {
                // Create a request for getting Embed token 
                // This method works only with new Power BI V2 workspace experience
                var tokenRequest = new GenerateTokenRequestV2(

                reports: new List<GenerateTokenRequestV2Report>() { new GenerateTokenRequestV2Report(reportId) },

                //datasets: datasetIds.Select(datasetId => new GenerateTokenRequestV2Dataset(datasetId.ToString())).ToList(),

                targetWorkspaces: targetWorkspaceId != Guid.Empty ? new List<GenerateTokenRequestV2TargetWorkspace>() { new GenerateTokenRequestV2TargetWorkspace(targetWorkspaceId) } : null
                );

                // Generate Embed token
                var embedToken = pbiClient.EmbedToken.GenerateToken(tokenRequest);

                return embedToken;
            }
        }




        private void PickerReportPages_SelectedIndexChanged(object sender, EventArgs e)
        {
            ChangePage(pickerReportPages.SelectedIndex);
        }

        HtmlWebViewSource LoadHTMLFileFromResource()
        {
            var source = new HtmlWebViewSource();

            // Load the HTML file embedded as a resource in the .NET Standard library
            var assembly = typeof(ReportViewer).GetTypeInfo().Assembly;
            var stream = assembly.GetManifestResourceStream("TimeTrackerBIXF.PowerBIViewer.html");
            using (var reader = new StreamReader(stream))
            {
                source.Html = reader.ReadToEnd();
            }
            return source;
        }

        private void ConfigureReportViewer()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                Configuring = true;

                var Token = EmbedConfig.EmbedToken.Token;
                var EmbedUrl = EmbedConfig.EmbedUrl;
                var Id = EmbedConfig.Id;

                string strColumn = "";
                string strTable = "";
                string strOperator = "";
                string strValues = "";

                if (EmbedConfig.Parameter)
                {
                    strColumn = EmbedConfig.PColumn;
                    strTable = EmbedConfig.PTable;

                    switch (EmbedConfig.PValue)
                    {
                        case "UserID":
                            strValues = App.CurrentUser.UserID.ToString();
                            strOperator = "Eq";

                            break;

                        case "LevelID":
                            strValues = App.CurrentUser.Levels;
                            strOperator = "In";

                            break;
                    }
                }


                string result = await _webView.EvaluateJavaScriptAsync($"SetTokens('{Token}','{EmbedUrl}','{Id}',{(EmbedConfig.Parameter ? 1 : 0)},'{EmbedConfig.PTable}','{EmbedConfig.PColumn}','{strOperator}','{strValues}')");

                Configuring = false;
                Configured = true;

                await Alerts.ShowLoadingPageAsync("Cargando reporte");

                StartTimer();
            });
        }



        #region C# => JS Calls

        private void GetReportDocReadyFlag()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                IsReadyChecking = true;

                string ReportDocRed = await _webView.EvaluateJavaScriptAsync("ReportDocRed()");
                if (string.IsNullOrEmpty(ReportDocRed))
                {
                    IsReadyChecking = false;
                    return;
                }
                if (ReportDocRed.Equals("4"))
                {
                    StopDocLoadedTimer();
                    GenerateEmbedToken();
                }


                IsReadyChecking = false;
            });
        }

        private void GetLoadedFlag()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                IsChecking = true;

                string Loaded = await _webView.EvaluateJavaScriptAsync("ReportLoaded()");
                if (Loaded.Equals("1"))
                {
                    StopTimer();
                    GetPages();
                    await Alerts.HideLoadingPageAsync();
                }
                else if (Loaded.Contains("2"))
                {
                    StopTimer();

                    string PBIError = Loaded.Replace("2", "");

                    await Alerts.HideLoadingPageAsync();

                    if (PBIError.Contains("Mobile layout was not found"))
                    {
                        Alerts.ShowAlert("Alerta", "No se encontró el layout para dispositivos móviles.Se usara la vista por default.");
                    }
                    else
                    {
                        Alerts.ShowAlert(string.Empty, "Error al cargar el reporte de PowerBI");
                    }


                }

                IsChecking = false;
            });
        }

        private void GetPages()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                string PagesIds = await _webView.EvaluateJavaScriptAsync("ReportPages()");
                string PagesDysplayNames = await _webView.EvaluateJavaScriptAsync("ReportPagesDisplayNames()");
                try
                {
                    ReportPages = JsonConvert.DeserializeObject<List<string>>(PagesDysplayNames);
                    ReportPagesIds = JsonConvert.DeserializeObject<List<string>>(PagesIds);

                    pickerReportPages.SelectedIndexChanged -= PickerReportPages_SelectedIndexChanged;

                    pickerReportPages.ItemsSource = ReportPages;
                    pickerReportPages.SelectedIndex = 0;

                    pickerReportPages.SelectedIndexChanged += PickerReportPages_SelectedIndexChanged;

                }
                catch (Exception x)
                {
                    Alerts.ShowAlert(string.Empty, "Error al obtener datos del reporte.");
                }

            });
        }

        #endregion C# => JS Calls



        #region Navigation Config

        private void RefreshClicked(object sender, EventArgs e)
        {
            App.Current.MainPage.Navigation.PopAsync(true);
        }

        void PreviousClicked(object sender, EventArgs e)
        {
            pickerReportPages.SelectedIndex = GetIndex(false);
        }

        void HomeClicked(object sender, EventArgs e)
        {
            pickerReportPages.SelectedIndex = 0;
        }


        void NextClicked(object sender, EventArgs e)
        {
            pickerReportPages.SelectedIndex = GetIndex(true);
        }

        void SettingsClicked(object sender, EventArgs e)
        {
            App.Current.MainPage.Navigation.PushAsync(new Settings(), true);
        }

        async void ChangePage(int Index)
        {
            string PageName = ReportPagesIds[Index];
            string response = await _webView.EvaluateJavaScriptAsync($"changePageByName('{PageName}')");
        }

        private int GetIndex(bool forwards)
        {
            int activeButtonIndex = pickerReportPages.SelectedIndex;

            if (forwards)
            {
                activeButtonIndex += 1;
            }
            else
            {
                activeButtonIndex -= 1;
            }

            if (activeButtonIndex > ReportPages.Count - 1)
            {
                activeButtonIndex = 0;
            }
            if (activeButtonIndex < 0)
            {
                activeButtonIndex = ReportPages.Count - 1;
            }

            return activeButtonIndex;
        }

        #endregion Navigation Config


        #region Timer Config
        private void StartTimer()
        {
            Timer_StatusCheck = Timer_StatusCheck ?? new Timer(2000);
            Timer_StatusCheck.Enabled = true;
            Timer_StatusCheck.AutoReset = true;
            Timer_StatusCheck.Elapsed += delegate
            {
                if (!IsChecking)
                {
                    GetLoadedFlag();
                }
            };
        }

        private void StopTimer()
        {
            if (Timer_StatusCheck != null)
            {
                Timer_StatusCheck.Enabled = false;
                Timer_StatusCheck = null;
            }
        }

        private void StartDocLoadedTimer()
        {
            Timer_DocLoadedCheck = Timer_DocLoadedCheck ?? new Timer(2000);
            Timer_DocLoadedCheck.Enabled = true;
            Timer_DocLoadedCheck.AutoReset = true;
            Timer_DocLoadedCheck.Elapsed += delegate
            {
                if (!IsReadyChecking)
                {
                    GetReportDocReadyFlag();
                }
            };
        }

        private void StopDocLoadedTimer()
        {
            if (Timer_DocLoadedCheck != null)
            {
                Timer_DocLoadedCheck.Enabled = false;
                Timer_DocLoadedCheck = null;
            }
        }

        #endregion Timer Config

        protected override bool OnBackButtonPressed()
        {
            StopTimer();
            StopDocLoadedTimer();
            return base.OnBackButtonPressed();
        }


    }
}