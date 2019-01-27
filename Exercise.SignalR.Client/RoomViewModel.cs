using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Exercise.SignalR.Client
{
    public class RoomViewModel : ObservableObject, IEquatable<RoomViewModel>
    {
        public ObservableCollection<string> Users { get; set; } = new ObservableCollection<string>();
        public string Name { get; set; }
        private StringBuilder _logWindow = new StringBuilder();

        public string Chat
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
