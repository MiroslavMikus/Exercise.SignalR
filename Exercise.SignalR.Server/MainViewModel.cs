using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Exercise.SignalR.Server
{
    public class MainViewModel : ViewModelBase
    {
        public ICommand StartCommmand { get; }

        public ICommand StopCommand { get; set; }
    }
}
