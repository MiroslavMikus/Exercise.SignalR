﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace Exercise.SignalR.Client
{
    public class NotEmptyString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string input)
            {
                return !string.IsNullOrEmpty(input);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
