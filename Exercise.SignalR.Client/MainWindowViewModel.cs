﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
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
        private const string SERVER_POSTGFIX = "signalr";
        private const string BASE_TITLE = "SignalR Client";

        private IHubProxy _hubProxy { get; set; }
        public string ServerUrl { get; set; } = "http://localhost:8080/";
        private readonly IDialogCoordinator _dialogCoordinator;

        public HubConnection Connection { get; set; }

        public string Title { get; set; } = BASE_TITLE;
        private string _name;

        private bool _isConnected = false;
        public bool IsConnected { get => _isConnected; set => Set(ref _isConnected, value); }
        private string _input;
        public string Input { get => _input; set => Set(ref _input, value); }

        private ObservableCollection<RoomViewModel> _rooms = new ObservableCollection<RoomViewModel>();
        public ObservableCollection<RoomViewModel> Rooms
        {
            get { return _rooms; }
            set { Set(ref _rooms, value); }
        }

        public ICommand SignInCommand { get; set; }
        public ICommand SignOutCommand { get; set; }
        public ICommand SendCommand { get; set; }
        public ICommand JoinCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand LeaveCommand { get; set; }

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

        public MainWindowViewModel(IDialogCoordinator dialogCoordinator)
        {
            _dialogCoordinator = dialogCoordinator;

            SignOutCommand = new RelayCommand(async () =>
            {
                foreach (var room in Rooms)
                {
                    await _hubProxy.Invoke("leaveRoom", room.Name);
                    room.Users.Clear();
                }

                Rooms.Clear();

                Connection.Stop();

                Connection.Dispose();

                IsConnected = false;

                Title = BASE_TITLE;
            });

            SignInCommand = new RelayCommand<string>(async a =>
            {
                var urlBuilder = new UriBuilder(ServerUrl)
                {
                    Path = SERVER_POSTGFIX
                };

                Connection = new HubConnection(urlBuilder.ToString());

                Connection.Closed += Connection_Closed;
                Connection.Reconnecting += Connection_Reconnecting;
                Connection.StateChanged += Connection_StateChanged;

                _hubProxy = Connection.CreateHubProxy("MessageHub");

                _name = Input;

                Title = $"{BASE_TITLE} [{_name}]";

                RaisePropertyChanged(nameof(Title));

                Input = string.Empty;

                SetupProxy(_hubProxy);

                try
                {
                    await Connection.Start();

                    var rooms = await _hubProxy.Invoke<Dictionary<string, List<string>>>("SignIn", _name);

                    foreach (var room in rooms)
                    {
                        EnsureRoom(room.Key).Users = new ObservableCollection<string>(room.Value);
                    }

                    IsConnected = true;

                    await _hubProxy.Invoke("joinRoom", "Main Group");
                }
                catch (Exception ex)
                {
                    LogWindow = $"Cant connect to {ServerUrl + SERVER_POSTGFIX}";
                }

            });

            SendCommand = new RelayCommand<string>(a =>
            {
                _hubProxy.Invoke("send", _name, a, Input);
                Input = string.Empty;
            });

            LeaveCommand = new RelayCommand<RoomViewModel>(async a =>
            {
                await _hubProxy.Invoke("leaveRoom", a.Name);
            });

            JoinCommand = new RelayCommand<string>(a =>
            {
                _hubProxy.Invoke("joinRoom", a);
            });

            AddCommand = new RelayCommand(async () =>
            {
                var mySettings = new MetroDialogSettings()
                {
                    DefaultButtonFocus = MessageDialogResult.Affirmative,
                    AffirmativeButtonText = "Create",
                    NegativeButtonText = "Cancel"
                };

                var input = await _dialogCoordinator.ShowInputAsync(this, "Create a new chatroom", "Enter room name:", mySettings);

                if (!string.IsNullOrEmpty(input))
                {
                    if (Rooms.Any(a => a.Name == input))
                    {
                        await _dialogCoordinator.ShowMessageAsync(this, "Error", $"Specified room: '{input}' already exist!");
                    }
                    else
                    {
                        JoinCommand.Execute(input);
                    }
                }
            });
        }

        private void SetupProxy(IHubProxy proxy)
        {
            proxy.On("addMessage", (Action<string, string, string>)((name, room, message) =>
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    EnsureRoom(room).Chat = $"{name}: {message}";
                });
            }));

            proxy.On<string, string>("UserJoinRoom", (room, user) =>
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    var chatRoom = EnsureRoom(room);
                    chatRoom.Users.Add(user);
                    chatRoom.Chat = $"User: '{user}' joined chat room.";

                    if (user == _name)
                        chatRoom.IsActive = true;
                });
            });

            proxy.On<string, string>("UserLeaveRoom", (room, user) =>
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    var chatRoom = EnsureRoom(room);
                    chatRoom.Users.Remove(user);
                    chatRoom.Chat = $"User: '{user}' leaved chat room.";

                    if (user == _name)
                        chatRoom.IsActive = false;

                    if (chatRoom.Users.Count == 0)
                    {
                        Rooms.Remove(chatRoom);
                    }
                });
            });
        }

        private RoomViewModel EnsureRoom(string room)
        {
            if (!Rooms.Any(b => b.Name == room))
            {
                Rooms.Add(new RoomViewModel()
                {
                    Name = room
                });
            }

            return Rooms.Single(b => b.Name.Equals(room));
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
