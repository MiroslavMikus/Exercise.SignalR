using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private ObservableCollection<RoomViewModel> _rooms;
        public ObservableCollection<RoomViewModel> Rooms
        {
            get { return _rooms; }
            set { Set(ref _rooms, value); }
        }

        public ICommand SignInCommand { get; set; }
        public ICommand SignOutCommand { get; set; }
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
            SignOutCommand = new RelayCommand(() =>
            {
                Connection.Stop();

                Connection.Dispose();

                IsConnected = false;
            });

            SignInCommand = new RelayCommand<string>(async a =>
            {
                Connection = new HubConnection(SERVER_URI);

                Connection.Closed += Connection_Closed;
                Connection.Reconnecting += Connection_Reconnecting;
                Connection.StateChanged += Connection_StateChanged;

                HubProxy = Connection.CreateHubProxy("MessageHub");

                _name = Input;

                Input = string.Empty;

                HubProxy.On<string, string, string>("addMessage", (name, room, message) =>
                {
                    if (!Rooms.Any(b => b.Name == room))
                    {
                        Rooms.Add(new RoomViewModel()
                        {
                            Name = room
                        });

                        Rooms.Single(b => b.Name == room).Chat = message;
                    }
                });

                HubProxy.On<>

                try
                {
                    await Connection.Start();

                    await HubProxy.Invoke("SignIn", _name);

                    IsConnected = true;
                }
                catch (Exception)
                {
                    LogWindow = $"Cant connect to {SERVER_URI}";
                }

            });

            SendCommand = new RelayCommand(() =>
            {
                HubProxy.Invoke("send", _name, Input);
                Input = string.Empty;
            });
        }

        private void Connection_StateChanged(StateChange state)
        {
            LogWindow = $"State changed from {state.OldState} to {state.NewState}";
        }

        private void Connection_Reconnecting()
        {
            LogWindow = "Trying to reconnect";
        }

        private void Connection_Closed()
        {
            IsConnected = false;
            LogWindow = "Connection was closed";
        }
    }
}
