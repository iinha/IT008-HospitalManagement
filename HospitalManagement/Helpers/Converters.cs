using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace HospitalManagement.Helpers
{
    public class StatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string status = value?.ToString()?.Trim().ToLower() ?? string.Empty;
            if (status.Contains("đã khám") || status.Contains("đã thanh toán") || status.Contains("hoàn thành") || status.Contains("trống") || status.Contains("đã trả lời"))
                return new SolidColorBrush(Color.FromRgb(25, 135, 84));
            if (status.Contains("đợi") || status.Contains("chờ") || status.Contains("đã đặt") || status.Contains("đang"))
                return new SolidColorBrush(Color.FromRgb(0, 86, 179));
            if (status.Contains("hủy") || status.Contains("chưa") || status.Contains("khóa"))
                return new SolidColorBrush(Color.FromRgb(220, 53, 69));
            return new SolidColorBrush(Color.FromRgb(33, 37, 41));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = value is bool b && b;
            if (parameter?.ToString() == "Invert")
                flag = !flag;
            return flag ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility v && v == Visibility.Visible;
        }
    }

    public class BoolToActiveTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool active = value is bool b && b;
            return active ? "Hoạt động" : "Đã khóa";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RoleToVietnameseConverter : IValueConverter
    {
        public static string Translate(string? role)
        {
            string r = role?.Trim().ToLower() ?? string.Empty;
            return r switch
            {
                "admin" => "Quản trị viên",
                "management" or "manager" => "Ban quản lý",
                "doctor" => "Bác sĩ",
                "nurse" => "Y tá / KTV",
                "reception" or "receptionist" => "Lễ tân",
                "pharmacy" or "pharmacist" => "Dược sĩ",
                "patient" => "Bệnh nhân",
                _ => role ?? string.Empty
            };
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Translate(value?.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}