using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;
using HospitalManagement.Helpers;
using HospitalManagement.Services;
using HospitalManagement.ViewModels.Admin;
using HospitalManagement.ViewModels.Doctor;
using HospitalManagement.ViewModels.Management;
using HospitalManagement.ViewModels.Nurse;
using HospitalManagement.ViewModels.Patient;
using HospitalManagement.ViewModels.Pharmacy;
using HospitalManagement.ViewModels.Reception;
using HospitalManagement.Views.Admin;
using HospitalManagement.Views.Doctor;
using HospitalManagement.Views.Management;
using HospitalManagement.Views.Nurse;
using HospitalManagement.Views.Patient;
using HospitalManagement.Views.Pharmacy;
using HospitalManagement.Views.Reception;
using HospitalManagement.Views.Shared;

namespace HospitalManagement.ViewModels
{
    public class NavItem
    {
        public string Title { get; set; } = string.Empty;
        public Func<UserControl> CreateView { get; set; } = null!;
    }

    public class MainViewModel : BaseViewModel
    {
        private readonly AuthService _authService = new();
        private UserControl? _currentView;
        private bool _isLoggedIn;
        private string _currentUserDisplay = string.Empty;
        private string _notificationMessage = "Hệ thống hoạt động bình thường. Không có cảnh báo cấp cứu.";
        private bool _showNotifyBar;

        public ObservableCollection<NavItem> MenuItems { get; } = new();

        public UserControl? CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => SetProperty(ref _isLoggedIn, value);
        }

        public string CurrentUserDisplay
        {
            get => _currentUserDisplay;
            set => SetProperty(ref _currentUserDisplay, value);
        }

        public string NotificationMessage
        {
            get => _notificationMessage;
            set => SetProperty(ref _notificationMessage, value);
        }

        public bool ShowNotifyBar
        {
            get => _showNotifyBar;
            set => SetProperty(ref _showNotifyBar, value);
        }

        public ICommand NavigateCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand BroadcastAlertCommand { get; }

        public MainViewModel()
        {
            NavigateCommand = new RelayCommand(param =>
            {
                if (param is NavItem item) CurrentView = item.CreateView();
            });
            LogoutCommand = new RelayCommand(_ => Logout());
            BroadcastAlertCommand = new RelayCommand(msg =>
            {
                if (msg is string text && !string.IsNullOrWhiteSpace(text))
                    NotificationMessage = $"[THÔNG BÁO KHẨN - {DateTime.Now:HH:mm}] {text}";
            });

            ShowLogin();
        }

        private void ShowLogin()
        {
            IsLoggedIn = false;
            ShowNotifyBar = false;
            MenuItems.Clear();
            var loginVm = new LoginViewModel(OnLoginSuccess);
            CurrentView = new LoginView { DataContext = loginVm };
        }

        private void OnLoginSuccess(AuthSession session)
        {
            IsLoggedIn = true;
            CurrentUserDisplay = $"{session.Fullname} ({session.Role})";
            MenuItems.Clear();

            string role = session.Role.Trim();
            ShowNotifyBar = !session.IsPatient;

            if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                var vm = new AdminViewModel();
                MenuItems.Add(new NavItem { Title = "Tài khoản & Phân quyền", CreateView = () => new AccountView { DataContext = vm } });
                MenuItems.Add(new NavItem { Title = "Sao lưu & Phục hồi", CreateView = () => new BackupView { DataContext = vm } });
                MenuItems.Add(new NavItem { Title = "Nhật ký hệ thống", CreateView = () => new LogView { DataContext = vm } });
            }
            else if (role.Equals("Management", StringComparison.OrdinalIgnoreCase) || role.Equals("Manager", StringComparison.OrdinalIgnoreCase))
            {
                var vm = new ManagementViewModel(msg => NotificationMessage = $"[CHỈ ĐẠO BQL - {DateTime.Now:HH:mm}] {msg}");
                MenuItems.Add(new NavItem { Title = "Tổng quan & Thống kê", CreateView = () => new DashView { DataContext = vm } });
                MenuItems.Add(new NavItem { Title = "Nhân sự & Lịch trực", CreateView = () => new StaffView { DataContext = vm } });
                MenuItems.Add(new NavItem { Title = "Thuốc & Dịch vụ", CreateView = () => new CatalogView { DataContext = vm } });
                MenuItems.Add(new NavItem { Title = "Quản trị nội dung", CreateView = () => new ContentView { DataContext = vm } });
            }
            else if (role.Equals("Doctor", StringComparison.OrdinalIgnoreCase))
            {
                var vm = new DoctorViewModel(session.Id);
                MenuItems.Add(new NavItem { Title = "Hàng đợi khám", CreateView = () => new QueueView { DataContext = vm } });
                MenuItems.Add(new NavItem { Title = "Khám khúc xạ & Ký kính", CreateView = () => new RefractView { DataContext = vm } });
                MenuItems.Add(new NavItem { Title = "Khám lâm sàng & Kê toa", CreateView = () => new ExaminationView { DataContext = vm } });
                MenuItems.Add(new NavItem { Title = "Phòng khám & Lịch trực", CreateView = () => new RoomView { DataContext = vm } });
                MenuItems.Add(new NavItem { Title = "Hỏi đáp bệnh nhân", CreateView = () => new Views.Doctor.QnAView { DataContext = vm } });
            }
            else if (role.Equals("Nurse", StringComparison.OrdinalIgnoreCase))
            {
                var vm = new NurseViewModel(session.Id);
                MenuItems.Add(new NavItem { Title = "Đo chỉ số ban đầu", CreateView = () => new MeasureView { DataContext = vm } });
                MenuItems.Add(new NavItem { Title = "Hướng dẫn cận lâm sàng", CreateView = () => new GuideView { DataContext = vm } });
                MenuItems.Add(new NavItem { Title = "Thực hiện y lệnh", CreateView = () => new CareView { DataContext = vm } });
            }
            else if (role.Equals("Reception", StringComparison.OrdinalIgnoreCase) || role.Equals("Receptionist", StringComparison.OrdinalIgnoreCase))
            {
                var vm = new ReceptionViewModel();
                MenuItems.Add(new NavItem { Title = "Tiếp nhận & Check-in", CreateView = () => new CheckinView { DataContext = vm } });
                MenuItems.Add(new NavItem { Title = "Thu ngân & Hóa đơn", CreateView = () => new BillView { DataContext = vm } });
            }
            else if (role.Equals("Pharmacy", StringComparison.OrdinalIgnoreCase) || role.Equals("Pharmacist", StringComparison.OrdinalIgnoreCase))
            {
                var vm = new PharmacyViewModel();
                MenuItems.Add(new NavItem { Title = "Cấp phát thuốc", CreateView = () => new DispenseView { DataContext = vm } });
            }
            else if (session.IsPatient)
            {
                var vm = new PatientViewModel(session.Id);
                MenuItems.Add(new NavItem { Title = "Đặt lịch khám & Hồ sơ", CreateView = () => new BookingView { DataContext = vm } });
                MenuItems.Add(new NavItem { Title = "Lịch sử & BHYT", CreateView = () => new HistoryView { DataContext = vm } });
                MenuItems.Add(new NavItem { Title = "Cổng thông tin & Tin tức", CreateView = () => new NewsView { DataContext = vm } });
                MenuItems.Add(new NavItem { Title = "Góc hỏi đáp bác sĩ", CreateView = () => new Views.Patient.QnAView { DataContext = vm } });
            }

            if (MenuItems.Count > 0)
                CurrentView = MenuItems[0].CreateView();
        }

        private void Logout()
        {
            _authService.Logout();
            ShowLogin();
        }
    }
}