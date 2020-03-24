using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Pages;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TimeTrackerBIXF.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Settings3Dialog : PopupPage
    {
        public Settings3Dialog()
        {
            InitializeComponent();

        }

        private void ClosePopUp_Clicked(object sender, System.EventArgs e)
        {
            Application.Current.MainPage.Navigation.PopPopupAsync(true);
        }

        private void Logout_Clicked(object sender, System.EventArgs e)
        {
            App.UsersB.DeleteAll();
            //Finish();
            Application.Current.MainPage.Navigation.PopPopupAsync(true);
            App.Current.MainPage = new Login();
        }
    }
}