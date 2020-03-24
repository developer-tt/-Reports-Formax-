using Microsoft.PowerBI.Api.V2;
using Microsoft.PowerBI.Api.V2.Models;
using Microsoft.Rest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

            GenerateEmbedToken();
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
                        EmbedToken tokenResponse = client.Reports.GenerateToken(EmbedConfig.GroupID, EmbedConfig.Id, new GenerateTokenRequest(accessLevel: "view"));

                        EmbedConfig.EmbedToken = tokenResponse;

                        App.EmbedTokenB.Create(new Data.Models.EmbedTokenDTO() { Expiration = tokenResponse.Expiration, Token = tokenResponse.Token, GroupID = EmbedConfig.GroupID, Id = EmbedConfig.Id, TokenId = tokenResponse.TokenId });


                        ConfigureReportViewer();
                    }
                    else
                    {
                        EmbedConfig.EmbedToken = new EmbedToken(TokenCached.Token, TokenCached.TokenId, TokenCached.Expiration);

                        ConfigureReportViewer();
                    }
                    
                         
                    if (!Configured)
                    {
                        StartDocLoadedTimer();
                    }

                }
            }
            catch (HttpOperationException ex)
            {
                var content = ex.Response.Content;
                Alerts.ShowAlert(string.Empty, "Error al mostrar el reporte.");
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

                string result = await _webView.EvaluateJavaScriptAsync($"SetTokens('{Token}','{EmbedUrl}','{Id}')");

                if (string.IsNullOrEmpty(result))
                {
                    Configuring = false;
                    return;
                }

                StopDocLoadedTimer();

                Configuring = false;
                Configured = true;

                await Alerts.ShowLoadingPageAsync("Cargando reporte");

                StartTimer();
            });
        }



        #region C# => JS Calls

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
                else if (Loaded.Equals("2"))
                {
                    StopTimer();
                    Alerts.ShowAlert(string.Empty, "Error al cargar el reporte de PowerBI");
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
            Timer_DocLoadedCheck = Timer_DocLoadedCheck ?? new Timer(1000);
            Timer_DocLoadedCheck.Enabled = true;
            Timer_DocLoadedCheck.AutoReset = true;
            Timer_DocLoadedCheck.Elapsed += delegate
            {
                if (!Configuring && !Configured)
                {
                    ConfigureReportViewer();
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