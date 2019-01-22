using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Exercise.SignalR.Client
{
    public class MainWindowViewModel : ViewModelBase
    {
        public IHubProxy HubProxy { get; set; }
        const string SERVER_URI = "http://localhost:8080/signalr";
        public HubConnection Connection { get; set; }

        private string _name;
        private bool _isConnected = false;
        public bool IsConnected { get => _isConnected; set => Set(ref _isConnected, value); }
        private string _input;
        public string Input { get => _input; set => Set(ref _input, value); }
        public ICommand SignInCommand { get; set; }
        public ICommand SendCommand { get; set; }

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

        public MainWindowViewModel()
        {
            SignInCommand = new RelayCommand<string>(async a =>
            {
                Connection = new HubConnection(SERVER_URI);
                Connection.Closed += Connection_Closed;
                HubProxy = Connection.CreateHubProxy("MessageHub");

                _name = Input;
                Input = string.Empty;

                HubProxy.On<string, string>("addMessage", (name, message) =>
                {
                    LogWindow = $"{name}: {message}";
                });

                await Connection.Start();

                IsConnected = true;
            });

            SendCommand = new RelayCommand(() =>
            {
                HubProxy.Invoke("send", _name, Input);
                Input = string.Empty;
            });
        }

        private void Connection_Closed()
        {
            IsConnected = false;
            LogWindow = "Connection was closed";
        }
    }
}
