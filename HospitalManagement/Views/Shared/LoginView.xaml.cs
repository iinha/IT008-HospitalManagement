using System.Windows;
using System.Windows.Controls;
using HospitalManagement.ViewModels;

namespace HospitalManagement.Views.Shared
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private void LoginPwdBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
                vm.Password = LoginPwdBox.Password;
        }

        private void RegPwdBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
                vm.RegPassword = RegPwdBox.Password;
        }
    }
}