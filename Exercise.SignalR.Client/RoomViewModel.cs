using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Exercise.SignalR.Client
{
    public class RoomViewModel : ObservableObject, IEquatable<RoomViewModel>
    {
        private ObservableCollection<string> _users = new ObservableCollection<string>();
        public ObservableCollection<string> Users { get => _users; set => Set(ref _users, value); }
        public string Name { get; set; }
        private StringBuilder _logWindow = new StringBuilder();

        private bool _isActive;

        public bool IsActive
        {
            get { return _isActive; }
            set { Set(ref _isActive, value); }
        }

        public string Chat
        {
            get
            {
                return _logWindow.ToString();
            }
            set
            {
                _logWindow.Insert(0, $"[{DateTime.Now.ToString("HH:mm:ss")}] {value}{Environment.NewLine}");
                RaisePropertyChanged();
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as RoomViewModel);
        }

        public bool Equals(RoomViewModel other)
        {
            return other != null && Name == other.Name;
        }

        public override int GetHashCode()
        {
            return 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);
        }
    }
}
