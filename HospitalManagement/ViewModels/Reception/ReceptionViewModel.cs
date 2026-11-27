using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using HospitalManagement.Helpers;
using HospitalManagement.Models;
using HospitalManagement.Services;

namespace HospitalManagement.ViewModels.Reception
{
    public class ReceptionViewModel : BaseViewModel
    {
        private readonly ExaminationService _examService = new();
        private readonly BillingService _billingService = new();

        private Appointment? _selectedAppointment;
        private Room? _selectedRoom;
        private Bill? _selectedBill;
        private decimal _examFee = 150000m;
        private string _statusMessage = string.Empty;

        public ObservableCollection<Appointment> Appointments { get; } = new();
        public ObservableCollection<Room> Rooms { get; } = new();
        public ObservableCollection<Bill> Bills { get; } = new();

        public Appointment? SelectedAppointment { get => _selectedAppointment; set => SetProperty(ref _selectedAppointment, value); }
        public Room? SelectedRoom { get => _selectedRoom; set => SetProperty(ref _selectedRoom, value); }
        public Bill? SelectedBill { get => _selectedBill; set => SetProperty(ref _selectedBill, value); }
        public decimal ExamFee { get => _examFee; set => SetProperty(ref _examFee, value); }
        public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }

        public ICommand RefreshCommand { get; }
        public ICommand CheckInCommand { get; }
        public ICommand PayBillCommand { get; }
        public ICommand PrintBillCommand { get; }

        public ReceptionViewModel()
        {
            RefreshCommand = new RelayCommand(_ => LoadAll());
            CheckInCommand = new RelayCommand(_ => CheckIn());
            PayBillCommand = new RelayCommand(_ => PayBill());
            PrintBillCommand = new RelayCommand(_ => PrintBill());
            LoadAll();
        }

        public void LoadAll()
        {
            Appointments.Clear();
            foreach (var a in _examService.GetAllPendingAppointments()) Appointments.Add(a);

            Rooms.Clear();
            foreach (var r in _examService.GetRooms()) Rooms.Add(r);

            Bills.Clear();
            foreach (var b in _billingService.GetAllBills()) Bills.Add(b);
        }

        private void CheckIn()
        {
            if (SelectedAppointment == null) return;
            var record = _examService.CheckInAppointment(
                SelectedAppointment.ApptId,
                SelectedAppointment.DoctorId,
                SelectedRoom?.RoomId,
                ExamFee);
            StatusMessage = $"Đã Check-in lịch #{SelectedAppointment.ApptId} -> Tạo hồ sơ #{record.RecordId} vào hàng đợi.";
            LoadAll();
        }

        private void PayBill()
        {
            if (SelectedBill == null) return;
            _billingService.PayBill(SelectedBill.BillId);
            StatusMessage = $"Đã xác nhận thanh toán hóa đơn #{SelectedBill.BillId}.";
            LoadAll();
        }

        private void PrintBill()
        {
            if (SelectedBill == null) return;
            string receipt = $"HÓA ĐƠN VIỆN PHÍ #{SelectedBill.BillId}\n" +
                             $"Hồ sơ: #{SelectedBill.RecordId}\n" +
                             $"Bệnh nhân: {SelectedBill.Record?.Patient?.Fullname}\n" +
                             $"Loại phí: {SelectedBill.BillType}\n" +
                             $"Tổng tiền: {SelectedBill.TotalAmount:N0} VNĐ\n" +
                             $"Trạng thái: {SelectedBill.Status}\n" +
                             $"Ngày tạo: {SelectedBill.CreatedAt:dd/MM/yyyy HH:mm}";
            MessageBox.Show(receipt, "In hóa đơn", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}