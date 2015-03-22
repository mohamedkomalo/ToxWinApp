using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ToxWinApp
{
    public class ToxAccount : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _Id;
        public string Id
        {
            get { return _Id; }
            set { _Id = value; NotifyPropertyChanged(); }
        }

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set { _Name = value; NotifyPropertyChanged(); }
        }

        private string _Status;
        public string Status
        {
            get { return _Status; }
            set { _Status = value; NotifyPropertyChanged(); }
        }

        private void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

}
