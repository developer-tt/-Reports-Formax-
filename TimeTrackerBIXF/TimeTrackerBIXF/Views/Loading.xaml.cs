using Rg.Plugins.Popup.Pages;
using Xamarin.Forms.Xaml;

namespace TimeTrackerBIXF.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Loading : PopupPage
    {
        public Loading(string Message)
        {
            InitializeComponent();

            LabelMessage.Text = Message;
        }


    }
}