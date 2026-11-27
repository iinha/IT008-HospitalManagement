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
    public class NavItem : BaseViewModel
    {
        private bool _isSelected;
        public string Title { get; set; } = string.Empty;
        public Func<UserControl> CreateView { get; set; } = null!;
        public Action? OnSelected { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }

    public class MainViewModel : BaseViewModel
    {
        private readonly AuthService _authService = new();
        private UserControl? _currentView;
        private bool _isLoggedIn;
        private string _currentUserDisplay = string.Empty;
        private string _notificationMessage = "Hệ thống hoạt động bình thường. Không có cảnh báo cấp cứu.";
        private bool _showNotifyBar;
        private bool _isAlertUrgent;

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

        public bool IsAlertUrgent
        {
            get => _isAlertUrgent;
            set => SetProperty(ref _isAlertUrgent, value);
        }

        public ICommand NavigateCommand { get; }
        public ICommand LogoutCommand { get; }

        public MainViewModel()
        {
            NavigateCommand = new RelayCommand(param =>
            {
                if (param is NavItem item) SelectNavItem(item);
            });
            LogoutCommand = new RelayCommand(_ => Logout());

            ShowLogin();
        }

        private void SelectNavItem(NavItem item)
        {
            foreach (var m in MenuItems) m.IsSelected = false;
            item.IsSelected = true;
            item.OnSelected?.Invoke();
            CurrentView = item.CreateView();
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
            string roleVi = RoleToVietnameseConverter.Translate(session.Role);
            CurrentUserDisplay = $"{session.Fullname} ({roleVi})";
            MenuItems.Clear();

            string role = session.Role.Trim();
            ShowNotifyBar = !session.IsPatient;

            if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                var vm = new AdminViewModel();
                MenuItems.Add(new NavItem { Title = "Tài khoản & Phân quyền", OnSelected = vm.RefreshAll, CreateView = () => new AccountView { DataContext = vm } });
                MenuItems.Add(new NavItem { Title = "Sao lưu & Phục hồi", CreateView = () => new BackupView { DataContext = vm } });
                MenuItems.Add(new NavItem { Title = "Nhật ký hệ thống", OnSelected = vm.RefreshAll, CreateView = () => new LogView { DataContext = vm } });
            }
            else if (role.Equals("Management", StringComparison.OrdinalIgnoreCase) || role.Equals("Manager", StringComparison.OrdinalIgnoreCase))
            {
                var vm = new ManagementViewModel(msg =>
                {
                    NotificationMessage = $"[CHỈ ĐẠO BQL - {DateTime.Now:HH:mm}] {msg}";
                    IsAlertUrgent = true;
                });
                MenuItems.Add(new NavItem { Title = "Tổng quan & Thống kê", OnSelected = vm.LoadAll, CreateView = () => new DashView { DataContext = vm } });
                MenuItems.Add(new NavItem { Title = "Nhân sự & Lịch trực", OnSelected = vm.LoadAll, CreateView = () => new StaffView { DataContext = vm } });
                MenuItems.Add(new NavItem { Title = "Thuốc & Dịch vụ", OnSelected = vm.LoadAll, CreateView = () => new CatalogView { DataContext = vm } });
                MenuItems.Add(new NavItem { Title = "Quản trị nội dung", OnSelected = vm.LoadAll, CreateView = () => new ContentView { DataContext = vm } });
            }
            else if (role.Equals("Doctor", StringComparison.OrdinalIgnoreCase))
            {
                var vm = new DoctorViewModel(session.Id);
                MenuItems.Add(new NavItem { Title = "Hàng đợi khám", OnSelected = vm.LoadAll, CreateView = () => new QueueView { DataContext = vm } });
                MenuItems.Add(new NavItem { Title = "Khám khúc xạ & Ký kính", OnSelected = vm.LoadAll, CreateView = () => new RefractView { DataContext = vm } });
                MenuItems.Add(new NavItem { Title = "Khám lâm sàng & Kê toa", OnSelected = vm.LoadAll, CreateView = () => new ExaminationView { DataContext = vm } });
                MenuItems.Add(new NavItem { Title = "Phòng khám & Lịch trực", OnSelected = vm.LoadAll, CreateView = () => new RoomView { DataContext = vm } });
                MenuItems.Add(new NavItem { Title = "Hỏi đáp bệnh nhân", OnSelected = vm.LoadAll, CreateView = () => new Views.Doctor.QnAView { DataContext = vm } });
            }
            else if (role.Equals("Nurse", StringComparison.OrdinalIgnoreCase))
            {
                var vm = new NurseViewModel(session.Id);
                MenuItems.Add(new NavItem { Title = "Đo chỉ số ban đầu", OnSelected = vm.LoadAll, CreateView = () => new MeasureView { DataContext = vm } });
                MenuItems.Add(new NavItem { Title = "Hướng dẫn cận lâm sàng", OnSelected = vm.LoadAll, CreateView = () => new GuideView { DataContext = vm } });
                MenuItems.Add(new NavItem { Title = "Thực hiện y lệnh", OnSelected = vm.LoadAll, CreateView = () => new CareView { DataContext = vm } });
            }
            else if (role.Equals("Reception", StringComparison.OrdinalIgnoreCase) || role.Equals("Receptionist", StringComparison.OrdinalIgnoreCase))
            {
                var vm = new ReceptionViewModel();
                MenuItems.Add(new NavItem { Title = "Tiếp nhận & Check-in", OnSelected = vm.LoadAll, CreateView = () => new CheckinView { DataContext = vm } });
                MenuItems.Add(new NavItem { Title = "Thu ngân & Hóa đơn", OnSelected = vm.LoadAll, CreateView = () => new BillView { DataContext = vm } });
            }
            else if (role.Equals("Pharmacy", StringComparison.OrdinalIgnoreCase) || role.Equals("Pharmacist", StringComparison.OrdinalIgnoreCase))
            {
                var vm = new PharmacyViewModel();
                MenuItems.Add(new NavItem { Title = "Cấp phát thuốc", OnSelected = vm.LoadData, CreateView = () => new DispenseView { DataContext = vm } });
            }
            else if (session.IsPatient)
            {
                var vm = new PatientViewModel(session.Id);
                MenuItems.Add(new NavItem { Title = "Đặt lịch khám & Hồ sơ", OnSelected = vm.LoadAll, CreateView = () => new BookingView { DataContext = vm } });
                MenuItems.Add(new NavItem { Title = "Lịch sử & BHYT", OnSelected = vm.LoadAll, CreateView = () => new HistoryView { DataContext = vm } });
                MenuItems.Add(new NavItem { Title = "Cổng thông tin & Tin tức", OnSelected = vm.LoadAll, CreateView = () => new NewsView { DataContext = vm } });
                MenuItems.Add(new NavItem { Title = "Góc hỏi đáp bác sĩ", OnSelected = vm.LoadAll, CreateView = () => new Views.Patient.QnAView { DataContext = vm } });
            }

            if (MenuItems.Count > 0)
                SelectNavItem(MenuItems[0]);
        }

        private void Logout()
        {
            _authService.Logout();
            ShowLogin();
        }
    }
}