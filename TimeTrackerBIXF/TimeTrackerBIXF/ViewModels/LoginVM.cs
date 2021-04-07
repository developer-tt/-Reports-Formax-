using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TimeTrackerBIXF.Data.AuxModels;
using TimeTrackerBIXF.Data.Models;
using TimeTrackerBIXF.DataServices;
using TimeTrackerBIXF.Helpers;
using TimeTrackerBIXF.Utils;
using TimeTrackerBIXF.Views;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace TimeTrackerBIXF.ViewModels
{


    public class LoginVM : BaseVM
    {
        public string user { get; set; }
        public string password { get; set; }

        UsersService UsersService;
        DevicesService DevicesService;
        public ICommand LogInCommand => new AsyncCommand(LogInAsync);


        public LoginVM()
        {
            UsersService = new UsersService();
            DevicesService = new DevicesService();

            var ds = DeviceDisplay.MainDisplayInfo.Density;

        }

        private async Task LogInAsync()
        {
            if (string.IsNullOrEmpty(user))
            {
                Alerts.ShowAlert("", "Favor de ingresar un usuario valido.");
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                Alerts.ShowAlert("", "Favor de ingresar la contraseña.");
                return;
            }

            tblLogin Login = new tblLogin
            {
                UserName = user,
                Password = password
            };


            await Alerts.ShowLoadingPageAsync();

            Response response = await UsersService.Login(Login.UserName, Login.Password);

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
                        string[] UserData = response.Data.ReplaceAndSplit();
                        if (UserData[0] == Constants.Error_Code)
                        {
                            await Alerts.HideLoadingPageAsync();
                            Alerts.ShowAlert(string.Empty, UserData[1]);
                            return;
                        }

                        tblUsersDTO User = new tblUsersDTO() { UserID = int.Parse(UserData[0]), Name = UserData[1], Code = UserData[2], Email = Login.UserName };
                        App.UsersB.DeleteAll();
                        App.UsersB.Create(User);

                        await Alerts.HideLoadingPageAsync();

                        await GetFormaxUserInfo();

                        RegisterDevice();
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


        }

        private async Task GetFormaxUserInfo()
        {
            Response response = await UsersService.FormaxGetUserInfo(App.CurrentUser.Email);
            if (response.Result != Result.NETWORK_UNAVAILABLE)
            {
                if (response.Result == Result.OK)
                {
                    if (response.Data != null)
                    {
                        WSFormaxUserInfo WSFormaxUserInfo = JsonConvert.DeserializeObject<WSFormaxUserInfo>(response.Data);

                        if (WSFormaxUserInfo.Success)
                        {

                            tblUsersDTO current = App.CurrentUser;
                            current.Levels = string.Join(",", WSFormaxUserInfo.LevelIDs);

                            App.UsersB.Update(current);

                        }
                    }
                }
            }
        }

        private async void RegisterDevice()
        {
            await Alerts.ShowLoadingPageAsync("Registrando dispositivo");


            tblUsersDTO User = App.CurrentUser;
            WSDevice WSDevice = new WSDevice();
            WSDevice.Name = string.Format("{0}_{1}", User.Name, App.GetDeviceName());
            WSDevice.Code = User.Code;
            WSDevice.DeviceTypeID = App.GetDeviceTypeID();
            WSDevice.AppVersion = App.GetAppVersion();
            WSDevice.Model = App.GetDeviceName();
            WSDevice.OSVersion = App.GetOSVersion();

            Response response = await DevicesService.RegisterDevice(WSDevice);

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
                    if (!string.IsNullOrEmpty(response.Data))
                    {
                        response.Data = response.Data.NormalizeResponse();
                        if (response.Data == Constants.Error_Code)
                        {
                            await Alerts.HideLoadingPageAsync();
                            Alerts.ShowAlert(string.Empty, response.Data);
                            return;
                        }

                        await Alerts.HideLoadingPageAsync();
                        User.DeviceID = int.Parse(response.Data);
                        App.UsersB.Update(User);

                        App.Current.MainPage = new Confirmation();
                    }
                    else
                    {
                        await Alerts.HideLoadingPageAsync();
                        Alerts.ShowAlert(string.Empty, "Error al obtener datos del dispositivo.");
                    }
                }
            }
            else
            {
                await Alerts.HideLoadingPageAsync();
                Alerts.ShowNetworkError();

            }
        }

    }
}
