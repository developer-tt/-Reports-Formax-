using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using TimeTrackerBIXF.Data.AuxModels;
using TimeTrackerBIXF.Data.Models;
using TimeTrackerBIXF.DataServices;
using TimeTrackerBIXF.Helpers;
using TimeTrackerBIXF.Utils;
using TimeTrackerBIXF.Views;
using Xamarin.Forms;

namespace TimeTrackerBIXF.ViewModels
{
    public class ConfirmationVM : BaseVM
    {
        private Timer Timer_StatusCheck;
        bool IsChecking = false;
        DevicesService DeviceService;
        public string CheckingMsg { get; set; }

        public ConfirmationVM()
        {
            DeviceService = new DevicesService();

            CheckingMsg = "Confirmando Cuenta";

            StartTimer();

            GetDeviceStatus();

        }

        private void StartTimer()
        {
            Timer_StatusCheck = Timer_StatusCheck ?? new Timer(30000);
            Timer_StatusCheck.Enabled = true;
            Timer_StatusCheck.AutoReset = true;
            Timer_StatusCheck.Elapsed += delegate
            {
                if (!IsChecking)
                {
                    GetDeviceStatus();
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

        private async void GetDeviceStatus()
        {
            IsChecking = true;

            await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(1));


            tblUsersDTO User = App.CurrentUser;

            Response response = await DeviceService.GetDeviceStatus(User.DeviceID);

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
                    Alerts.ShowAlert(string.Empty, "Error al obtener datos del servidor (SX)");
                }
                else if (response.Result == Result.OK)
                {
                    if (!string.IsNullOrEmpty(response.Data))
                    {
                        string responseNormalized = response.Data.NormalizeResponse();

                        string[] UserData = response.Data.ReplaceAndSplit();
                        if (responseNormalized == Constants.Error_Code)
                        {
                            await Alerts.HideLoadingPageAsync();
                            Alerts.ShowAlert(string.Empty, "Error al registrar el dispositivo en el servidor,intente mas tarde.");
                            return;
                        }

                        User.DeviceStatus = responseNormalized;
                        App.UsersB.Update(User);

                        if (User.DeviceStatus == DeviceStatus.Approved)
                        {
                            Device.BeginInvokeOnMainThread(async () =>
                            {

                                StopTimer();
                                CheckingMsg = "Dispositivo aprobado";

                                await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(2));

                                App.Current.MainPage = new NavigationPage(new Reports());
                            });

                        }
                        else if (User.DeviceStatus == DeviceStatus.Rejected)
                        {
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                StopTimer();

                                CheckingMsg = "Dispositivo rechazado";

                                await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(2));

                                App.Current.MainPage = new Login();
                            });
                        }
                    }
                    else
                    {
                        await Alerts.HideLoadingPageAsync();
                        Alerts.ShowAlert(string.Empty, "Los datos obtenidos son invalidos.");
                    }
                }
            }
            else
            {
                await Alerts.HideLoadingPageAsync();
                Alerts.ShowNetworkError();
            }


            IsChecking = false;
        }
    }
}
