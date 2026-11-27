using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using HospitalManagement.Helpers;
using HospitalManagement.Models;
using HospitalManagement.Services;

namespace HospitalManagement.ViewModels.Management
{
    public class ManagementViewModel : BaseViewModel
    {
        private readonly AdminService _adminService = new();
        private readonly ExaminationService _examService = new();
        private readonly Action<string> _broadcastAlert;

        private decimal _totalRevenue;
        private int _totalPatients;
        private int _totalRecords;
        private int _totalAppointments;
        private string _alertText = string.Empty;

        private User? _selectedStaff;
        private string _staffUsername = string.Empty;
        private string _staffPassword = string.Empty;
        private string _staffFullname = string.Empty;
        private string _staffRole = "Doctor";

        private Room? _selectedRoom;
        private DateTime _shiftDate = DateTime.Today;
        private string _shiftTimeFrame = "Sang 07:30-11:30";

        private Drug? _selectedDrug;
        private string _drugName = string.Empty;
        private string _drugUnit = "Viên";
        private decimal _drugPrice;
        private int _drugStock;

        private Service? _selectedService;
        private string _serviceName = string.Empty;
        private decimal _servicePrice;
        private string _serviceCategory = "Khám bệnh";

        private Article? _selectedArticle;
        private string _articleTitle = string.Empty;
        private string _articleCategory = "Tin tức sức khỏe";
        private string _articleContent = string.Empty;

        public ObservableCollection<DoctorPerformanceDto> DoctorPerformances { get; } = new();
        public ObservableCollection<RevenueByDateDto> RevenueByDates { get; } = new();
        public ObservableCollection<User> StaffList { get; } = new();
        public ObservableCollection<Models.Patient> Patients { get; } = new();
        public ObservableCollection<Room> Rooms { get; } = new();
        public ObservableCollection<Shift> Shifts { get; } = new();
        public ObservableCollection<Drug> Drugs { get; } = new();
        public ObservableCollection<Service> Services { get; } = new();
        public ObservableCollection<Article> Articles { get; } = new();

        public string[] RoleOptions { get; } = { "Doctor", "Nurse", "Reception", "Pharmacy", "Management" };
        public string[] TimeFrames { get; } = { "Sang 07:30-11:30", "Chieu 13:30-17:30", "Toi 18:00-21:00" };
        public string[] ArticleCategories { get; } = { "Giới thiệu", "Cơ sở vật chất", "Đội ngũ bác sĩ", "Tin tức sức khỏe" };

        public decimal TotalRevenue { get => _totalRevenue; set => SetProperty(ref _totalRevenue, value); }
        public int TotalPatients { get => _totalPatients; set => SetProperty(ref _totalPatients, value); }
        public int TotalRecords { get => _totalRecords; set => SetProperty(ref _totalRecords, value); }
        public int TotalAppointments { get => _totalAppointments; set => SetProperty(ref _totalAppointments, value); }
        public string AlertText { get => _alertText; set => SetProperty(ref _alertText, value); }

        public User? SelectedStaff
        {
            get => _selectedStaff;
            set
            {
                if (SetProperty(ref _selectedStaff, value) && value != null)
                {
                    StaffUsername = value.Username ?? string.Empty;
                    StaffFullname = value.Fullname ?? string.Empty;
                    StaffRole = value.Role ?? "Doctor";
                }
            }
        }

        public string StaffUsername { get => _staffUsername; set => SetProperty(ref _staffUsername, value); }
        public string StaffPassword { get => _staffPassword; set => SetProperty(ref _staffPassword, value); }
        public string StaffFullname { get => _staffFullname; set => SetProperty(ref _staffFullname, value); }
        public string StaffRole { get => _staffRole; set => SetProperty(ref _staffRole, value); }

        public Room? SelectedRoom { get => _selectedRoom; set => SetProperty(ref _selectedRoom, value); }
        public DateTime ShiftDate { get => _shiftDate; set => SetProperty(ref _shiftDate, value); }
        public string ShiftTimeFrame { get => _shiftTimeFrame; set => SetProperty(ref _shiftTimeFrame, value); }

        public Drug? SelectedDrug
        {
            get => _selectedDrug;
            set
            {
                if (SetProperty(ref _selectedDrug, value) && value != null)
                {
                    DrugName = value.DrugName ?? string.Empty;
                    DrugUnit = value.Unit ?? string.Empty;
                    DrugPrice = value.UnitPrice ?? 0m;
                    DrugStock = value.StockQuantity ?? 0;
                }
            }
        }

        public string DrugName { get => _drugName; set => SetProperty(ref _drugName, value); }
        public string DrugUnit { get => _drugUnit; set => SetProperty(ref _drugUnit, value); }
        public decimal DrugPrice { get => _drugPrice; set => SetProperty(ref _drugPrice, value); }
        public int DrugStock { get => _drugStock; set => SetProperty(ref _drugStock, value); }

        public Service? SelectedService
        {
            get => _selectedService;
            set
            {
                if (SetProperty(ref _selectedService, value) && value != null)
                {
                    ServiceName = value.ServiceName ?? string.Empty;
                    ServicePrice = value.Price ?? 0m;
                    ServiceCategory = value.Category ?? string.Empty;
                }
            }
        }

        public string ServiceName { get => _serviceName; set => SetProperty(ref _serviceName, value); }
        public decimal ServicePrice { get => _servicePrice; set => SetProperty(ref _servicePrice, value); }
        public string ServiceCategory { get => _serviceCategory; set => SetProperty(ref _serviceCategory, value); }

        public Article? SelectedArticle
        {
            get => _selectedArticle;
            set
            {
                if (SetProperty(ref _selectedArticle, value) && value != null)
                {
                    ArticleTitle = value.Title ?? string.Empty;
                    ArticleCategory = value.Category ?? "Tin tức sức khỏe";
                    ArticleContent = value.Content ?? string.Empty;
                }
            }
        }

        public string ArticleTitle { get => _articleTitle; set => SetProperty(ref _articleTitle, value); }
        public string ArticleCategory { get => _articleCategory; set => SetProperty(ref _articleCategory, value); }
        public string ArticleContent { get => _articleContent; set => SetProperty(ref _articleContent, value); }

        public ICommand RefreshDashboardCommand { get; }
        public ICommand SendAlertCommand { get; }
        public ICommand AddStaffCommand { get; }
        public ICommand UpdateStaffCommand { get; }
        public ICommand ToggleStaffLockCommand { get; }
        public ICommand AssignShiftCommand { get; }
        public ICommand SaveDrugCommand { get; }
        public ICommand NewDrugCommand { get; }
        public ICommand DeleteDrugCommand { get; }
        public ICommand SaveServiceCommand { get; }
        public ICommand NewServiceCommand { get; }
        public ICommand DeleteServiceCommand { get; }
        public ICommand SaveArticleCommand { get; }
        public ICommand NewArticleCommand { get; }
        public ICommand DeleteArticleCommand { get; }

        public ManagementViewModel(Action<string> broadcastAlert)
        {
            _broadcastAlert = broadcastAlert;

            RefreshDashboardCommand = new RelayCommand(_ => LoadDashboard());
            SendAlertCommand = new RelayCommand(_ =>
            {
                if (!string.IsNullOrWhiteSpace(AlertText))
                {
                    _broadcastAlert(AlertText);
                    AlertText = string.Empty;
                }
            });

            AddStaffCommand = new RelayCommand(_ => AddStaff());
            UpdateStaffCommand = new RelayCommand(_ => UpdateStaff());
            ToggleStaffLockCommand = new RelayCommand(_ =>
            {
                if (SelectedStaff == null) return;
                _adminService.ToggleUserLock(SelectedStaff.UserId);
                LoadStaff();
            });
            AssignShiftCommand = new RelayCommand(_ => AssignShift());

            SaveDrugCommand = new RelayCommand(_ => SaveDrug());
            NewDrugCommand = new RelayCommand(_ => { SelectedDrug = null; DrugName = ""; DrugUnit = "Viên"; DrugPrice = 0; DrugStock = 0; });
            DeleteDrugCommand = new RelayCommand(_ => { if (SelectedDrug != null) { _adminService.DeleteDrug(SelectedDrug.DrugId); LoadCatalog(); } });

            SaveServiceCommand = new RelayCommand(_ => SaveService());
            NewServiceCommand = new RelayCommand(_ => { SelectedService = null; ServiceName = ""; ServicePrice = 0; ServiceCategory = "Khám bệnh"; });
            DeleteServiceCommand = new RelayCommand(_ => { if (SelectedService != null) { _adminService.DeleteService(SelectedService.ServiceId); LoadCatalog(); } });

            SaveArticleCommand = new RelayCommand(_ => SaveArticle());
            NewArticleCommand = new RelayCommand(_ => { SelectedArticle = null; ArticleTitle = ""; ArticleContent = ""; });
            DeleteArticleCommand = new RelayCommand(_ => { if (SelectedArticle != null) { _adminService.DeleteArticle(SelectedArticle.ArticleId); LoadArticles(); } });

            LoadAll();
        }

        private void LoadAll()
        {
            LoadDashboard();
            LoadStaff();
            LoadCatalog();
            LoadArticles();
        }

        private void LoadDashboard()
        {
            var m = _adminService.GetDashboardMetrics();
            TotalRevenue = m.TotalRevenue;
            TotalPatients = m.TotalPatients;
            TotalRecords = m.TotalRecords;
            TotalAppointments = m.TotalAppointments;

            DoctorPerformances.Clear();
            foreach (var d in _adminService.GetDoctorPerformance()) DoctorPerformances.Add(d);

            RevenueByDates.Clear();
            foreach (var r in _adminService.GetRevenueByDate()) RevenueByDates.Add(r);
        }

        private void LoadStaff()
        {
            StaffList.Clear();
            foreach (var u in _adminService.GetAllUsers()) StaffList.Add(u);

            Patients.Clear();
            foreach (var p in _adminService.GetAllPatients()) Patients.Add(p);

            Rooms.Clear();
            foreach (var r in _adminService.GetAllRooms()) Rooms.Add(r);

            Shifts.Clear();
            foreach (var s in _examService.GetShifts()) Shifts.Add(s);
        }

        private void LoadCatalog()
        {
            Drugs.Clear();
            foreach (var d in _adminService.GetAllDrugs()) Drugs.Add(d);

            Services.Clear();
            foreach (var s in _adminService.GetAllServices()) Services.Add(s);
        }

        private void LoadArticles()
        {
            Articles.Clear();
            foreach (var a in _adminService.GetAllArticles()) Articles.Add(a);
        }

        private void AddStaff()
        {
            if (string.IsNullOrWhiteSpace(StaffUsername) || string.IsNullOrWhiteSpace(StaffPassword) || string.IsNullOrWhiteSpace(StaffFullname)) return;
            _adminService.CreateUser(StaffUsername.Trim(), StaffPassword, StaffFullname.Trim(), StaffRole);
            StaffUsername = ""; StaffPassword = ""; StaffFullname = "";
            LoadStaff();
        }

        private void UpdateStaff()
        {
            if (SelectedStaff == null) return;
            SelectedStaff.Fullname = StaffFullname;
            SelectedStaff.Role = StaffRole;
            _adminService.UpdateUser(SelectedStaff);
            LoadStaff();
        }

        private void AssignShift()
        {
            if (SelectedStaff == null || SelectedRoom == null) return;
            _examService.RegisterShift(SelectedStaff.UserId, SelectedRoom.RoomId, ShiftDate, ShiftTimeFrame);
            LoadStaff();
        }

        private void SaveDrug()
        {
            if (string.IsNullOrWhiteSpace(DrugName)) return;
            var drug = new Drug
            {
                DrugId = SelectedDrug?.DrugId ?? 0,
                DrugName = DrugName.Trim(),
                Unit = DrugUnit.Trim(),
                UnitPrice = DrugPrice,
                StockQuantity = DrugStock
            };
            _adminService.SaveDrug(drug);
            LoadCatalog();
        }

        private void SaveService()
        {
            if (string.IsNullOrWhiteSpace(ServiceName)) return;
            var svc = new Service
            {
                ServiceId = SelectedService?.ServiceId ?? 0,
                ServiceName = ServiceName.Trim(),
                Price = ServicePrice,
                Category = ServiceCategory.Trim()
            };
            _adminService.SaveService(svc);
            LoadCatalog();
        }

        private void SaveArticle()
        {
            if (string.IsNullOrWhiteSpace(ArticleTitle)) return;
            var art = new Article
            {
                ArticleId = SelectedArticle?.ArticleId ?? 0,
                Title = ArticleTitle.Trim(),
                Category = ArticleCategory,
                Content = ArticleContent
            };
            _adminService.SaveArticle(art);
            LoadArticles();
        }
    }
}