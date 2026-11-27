using System;
using System.Text.RegularExpressions;
using System.Windows.Input;
using HospitalManagement.Helpers;
using HospitalManagement.Services;

namespace HospitalManagement.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly AuthService _authService = new();
        private readonly Action<AuthSession> _onLoginSuccess;

        private bool _isPatientMode = true;
        private bool _isRegisterMode;
        private string _usernameOrPhone = string.Empty;
        private string _password = string.Empty;
        private string _regFullname = string.Empty;
        private string _regPhone = string.Empty;
        private string _regPassword = string.Empty;
        private DateTime? _regDob = DateTime.Today.AddYears(-25);
        private string _regGender = "Nam";
        private string _regBhyt = string.Empty;
        private string _statusMessage = string.Empty;

        public bool IsPatientMode
        {
            get => _isPatientMode;
            set
            {
                if (SetProperty(ref _isPatientMode, value))
                {
                    OnPropertyChanged(nameof(IsStaffMode));
                    OnPropertyChanged(nameof(LoginIdentifierLabel));
                }
            }
        }

        public bool IsStaffMode
        {
            get => !_isPatientMode;
            set
            {
                if (value == _isPatientMode)
                    IsPatientMode = !value;
            }
        }

        public bool IsRegisterMode
        {
            get => _isRegisterMode;
            set => SetProperty(ref _isRegisterMode, value);
        }

        public string LoginIdentifierLabel => IsPatientMode ? "Số điện thoại:" : "Tên đăng nhập:";

        public string UsernameOrPhone
        {
            get => _usernameOrPhone;
            set => SetProperty(ref _usernameOrPhone, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string RegFullname { get => _regFullname; set => SetProperty(ref _regFullname, value); }
        public string RegPhone { get => _regPhone; set => SetProperty(ref _regPhone, value); }
        public string RegPassword { get => _regPassword; set => SetProperty(ref _regPassword, value); }
        public DateTime? RegDob { get => _regDob; set => SetProperty(ref _regDob, value); }
        public string RegGender { get => _regGender; set => SetProperty(ref _regGender, value); }
        public string RegBhyt { get => _regBhyt; set => SetProperty(ref _regBhyt, value); }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public string[] GenderOptions { get; } = { "Nam", "Nữ", "Khác" };

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand ToggleRegisterCommand { get; }

        public LoginViewModel(Action<AuthSession> onLoginSuccess)
        {
            _onLoginSuccess = onLoginSuccess;
            LoginCommand = new RelayCommand(_ => ExecuteLogin());
            RegisterCommand = new RelayCommand(_ => ExecuteRegister());
            ToggleRegisterCommand = new RelayCommand(_ =>
            {
                IsRegisterMode = !IsRegisterMode;
                StatusMessage = string.Empty;
            });
        }

        private static bool IsValidPhone(string phone)
        {
            return Regex.IsMatch(phone, @"^\d{9,11}$");
        }

        private void ExecuteLogin()
        {
            string identifier = UsernameOrPhone.Trim();
            if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrWhiteSpace(Password))
            {
                StatusMessage = "Vui lòng nhập đầy đủ thông tin đăng nhập.";
                return;
            }

            if (IsPatientMode && !IsValidPhone(identifier))
            {
                StatusMessage = "Số điện thoại không hợp lệ (chỉ gồm 9-11 chữ số).";
                return;
            }

            AuthSession? session = IsPatientMode
                ? _authService.LoginPatient(identifier, Password)
                : _authService.LoginStaff(identifier, Password);

            if (session == null)
            {
                StatusMessage = "Sai thông tin đăng nhập hoặc tài khoản đã bị khóa.";
                return;
            }

            StatusMessage = string.Empty;
            _onLoginSuccess(session);
        }

        private void ExecuteRegister()
        {
            string phone = RegPhone.Trim();
            if (string.IsNullOrWhiteSpace(RegFullname) || string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(RegPassword))
            {
                StatusMessage = "Vui lòng nhập Họ tên, Số điện thoại và Mật khẩu.";
                return;
            }

            if (!IsValidPhone(phone))
            {
                StatusMessage = "Số điện thoại phải gồm từ 9 đến 11 chữ số.";
                return;
            }

            if (RegPassword.Length < 4)
            {
                StatusMessage = "Mật khẩu phải có ít nhất 4 ký tự.";
                return;
            }

            if (RegDob.HasValue && RegDob.Value > DateTime.Today)
            {
                StatusMessage = "Ngày sinh không được lớn hơn ngày hiện tại.";
                return;
            }

            if (_authService.RegisterPatient(RegFullname.Trim(), phone, RegPassword, RegDob, RegGender, RegBhyt.Trim(), out string error))
            {
                StatusMessage = "Đăng ký thành công. Vui lòng đăng nhập.";
                UsernameOrPhone = phone;
                IsPatientMode = true;
                IsRegisterMode = false;
            }
            else
            {
                StatusMessage = error;
            }
        }
    }
}