using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTrackerBIXF.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TimeTrackerBIXF.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Settings : ContentPage
    {
        public Settings()
        {
            InitializeComponent();

            BindingContext = new SettingsVM();

        }
    }
}