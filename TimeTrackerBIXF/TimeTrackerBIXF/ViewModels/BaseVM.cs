using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace TimeTrackerBIXF.ViewModels
{
    public abstract class BaseVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        static BaseVM()
        {
        }

    }
}
