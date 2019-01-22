using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Exercise.SignalR.Server
{
    /// <summary>
    /// IOC https://docs.microsoft.com/en-us/aspnet/signalr/overview/advanced/dependency-injection
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        const string SERVER_URI = "http://localhost:8080";
        public IDisposable SigmalRHost { get; set; }

        private bool _isRunning = false;
        public bool IsRunning { get => _isRunning; set => Set(ref _isRunning, value); }

        private IHubContext _hubContext;

        public RelayCommand StartCommmand { get; }

        public RelayCommand StopCommand { get; }

        private StringBuilder _logWindow = new StringBuilder();

        public string LogWindow
        {
            get
            {
                return _logWindow.ToString();
            }
            set
            {
                _logWindow.Insert(0, $"{DateTime.Now.ToString("HH:mm:ss:FF")}: {value}{Environment.NewLine}");
                RaisePropertyChanged();
            }
        }

        public MainViewModel()
        {

            _hubContext = GlobalHost.ConnectionManager.GetHubContext<MessageHub>();

            StartCommmand = new RelayCommand(() =>
            {
                SigmalRHost = WebApp.Start(SERVER_URI);

                LogWindow = $"Server is running at {SERVER_URI}";

                IsRunning = true;
            });

            StopCommand = new RelayCommand(() =>
            {
                SigmalRHost.Dispose();

                LogWindow = $"Server was stopped";

                IsRunning = false;
            });
        }
    }
}
