using Rg.Plugins.Popup.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using TimeTrackerBIXF.Views;
using Xamarin.Forms;

namespace TimeTrackerBIXF.ViewModels
{
    public class SettingsVM : BaseVM
    {
        public string Name { get; set; }
        public string AppVersion { get; set; }

        public ICommand BackPressedCommand => new Command(BackPressed);
        public ICommand Settings2Command => new Command(Settings2);


        public ICommand Settings3Command => new Command(Settings3);


        public SettingsVM()
        {
            Name = App.CurrentUser.Name;

            AppVersion = App.GetAppVersion();

        }

        private void BackPressed()
        {
            App.Current.MainPage.Navigation.PopAsync();
        }

        private void Settings2()
        {
            App.Current.MainPage.Navigation.PushPopupAsync(new Settings2Dialog(),true);
        }


        private void Settings3()
        {
            App.Current.MainPage.Navigation.PushPopupAsync(new Settings3Dialog(), true);
        }




    }
}
