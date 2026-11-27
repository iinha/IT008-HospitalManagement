using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using HospitalManagement.Helpers;
using HospitalManagement.Models;
using HospitalManagement.Services;

namespace HospitalManagement.ViewModels.Doctor
{
    public class DoctorViewModel : BaseViewModel
    {
        private readonly ExaminationService _examService = new();
        private readonly int _doctorId;

        private Medicalrecord? _selectedRecord;
        private string _symptoms = string.Empty;
        private string _diagnosis = string.Empty;
        private bool _isInpatient;
        private string _examOutcome = "Đã khám";

        private double? _visionRight;
        private double? _visionLeft;
        private double? _iopRight;
        private double? _iopLeft;
        private double? _sphRight;
        private double? _cylRight;
        private double? _axisRight;
        private double? _sphLeft;
        private double? _cylLeft;
        private double? _axisLeft;

        private Service? _selectedService;
        private Drug? _selectedDrug;
        private int _drugQty = 1;
        private string _drugDosage = "Ngày nhỏ 3 lần, mỗi lần 1 giọt";
        private string _careActionName = "Nhỏ thuốc giãn đồng tử";

        private Room? _selectedRoom;
        private DateTime _shiftDate = DateTime.Today;
        private string _shiftTimeFrame = "07:30 - 11:30";

        private Question? _selectedQuestion;
        private string _answerText = string.Empty;
        private string _statusMessage = string.Empty;

        public ObservableCollection<Medicalrecord> QueueRecords { get; } = new();
        public ObservableCollection<Service> Services { get; } = new();
        public ObservableCollection<Drug> Drugs { get; } = new();
        public ObservableCollection<Room> Rooms { get; } = new();
        public ObservableCollection<Shift> MyShifts { get; } = new();
        public ObservableCollection<Question> Questions { get; } = new();
        public ObservableCollection<Servicedetail> CurrentRecordServices { get; } = new();
        public ObservableCollection<Prescriptiondetail> CurrentRecordPrescriptions { get; } = new();

        public string[] OutcomeOptions { get; } = { "Đã khám", "Chuyển thủ thuật", "Chỉ định nhập viện" };
        public string[] TimeFrames { get; } = { "07:30 - 11:30", "13:30 - 17:30", "18:00 - 21:00" };

        public Medicalrecord? SelectedRecord
        {
            get => _selectedRecord;
            set
            {
                if (SetProperty(ref _selectedRecord, value))
                    PopulateSelectedRecord();
            }
        }

        public string Symptoms { get => _symptoms; set => SetProperty(ref _symptoms, value); }
        public string Diagnosis { get => _diagnosis; set => SetProperty(ref _diagnosis, value); }
        public bool IsInpatient { get => _isInpatient; set => SetProperty(ref _isInpatient, value); }
        public string ExamOutcome { get => _examOutcome; set => SetProperty(ref _examOutcome, value); }

        public double? VisionRight { get => _visionRight; set => SetProperty(ref _visionRight, value); }
        public double? VisionLeft { get => _visionLeft; set => SetProperty(ref _visionLeft, value); }
        public double? IopRight { get => _iopRight; set => SetProperty(ref _iopRight, value); }
        public double? IopLeft { get => _iopLeft; set => SetProperty(ref _iopLeft, value); }
        public double? SphRight { get => _sphRight; set => SetProperty(ref _sphRight, value); }
        public double? CylRight { get => _cylRight; set => SetProperty(ref _cylRight, value); }
        public double? AxisRight { get => _axisRight; set => SetProperty(ref _axisRight, value); }
        public double? SphLeft { get => _sphLeft; set => SetProperty(ref _sphLeft, value); }
        public double? CylLeft { get => _cylLeft; set => SetProperty(ref _cylLeft, value); }
        public double? AxisLeft { get => _axisLeft; set => SetProperty(ref _axisLeft, value); }

        public Service? SelectedService { get => _selectedService; set => SetProperty(ref _selectedService, value); }
        public Drug? SelectedDrug { get => _selectedDrug; set => SetProperty(ref _selectedDrug, value); }
        public int DrugQty { get => _drugQty; set => SetProperty(ref _drugQty, value); }
        public string DrugDosage { get => _drugDosage; set => SetProperty(ref _drugDosage, value); }
        public string CareActionName { get => _careActionName; set => SetProperty(ref _careActionName, value); }

        public Room? SelectedRoom { get => _selectedRoom; set => SetProperty(ref _selectedRoom, value); }
        public DateTime ShiftDate { get => _shiftDate; set => SetProperty(ref _shiftDate, value); }
        public string ShiftTimeFrame { get => _shiftTimeFrame; set => SetProperty(ref _shiftTimeFrame, value); }

        public Question? SelectedQuestion
        {
            get => _selectedQuestion;
            set
            {
                if (SetProperty(ref _selectedQuestion, value) && value != null)
                    AnswerText = value.AnswerContent ?? string.Empty;
            }
        }

        public string AnswerText { get => _answerText; set => SetProperty(ref _answerText, value); }
        public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }

        public ICommand RefreshQueueCommand { get; }
        public ICommand SignRefractionCommand { get; }
        public ICommand OrderServiceCommand { get; }
        public ICommand AddDrugCommand { get; }
        public ICommand AddCareOrderCommand { get; }
        public ICommand FinishExamCommand { get; }
        public ICommand RegisterShiftCommand { get; }
        public ICommand ToggleRoomStatusCommand { get; }
        public ICommand ReplyQuestionCommand { get; }

        public DoctorViewModel(int doctorId)
        {
            _doctorId = doctorId;

            RefreshQueueCommand = new RelayCommand(_ => LoadAll());
            SignRefractionCommand = new RelayCommand(_ => SignRefraction());
            OrderServiceCommand = new RelayCommand(_ => OrderService());
            AddDrugCommand = new RelayCommand(_ => AddDrug());
            AddCareOrderCommand = new RelayCommand(_ => AddCareOrder());
            FinishExamCommand = new RelayCommand(_ => FinishExam());
            RegisterShiftCommand = new RelayCommand(_ => RegisterShift());
            ToggleRoomStatusCommand = new RelayCommand(_ => ToggleRoomStatus());
            ReplyQuestionCommand = new RelayCommand(_ => ReplyQuestion());

            LoadAll();
        }

        public void LoadAll()
        {
            int? selectedId = SelectedRecord?.RecordId;

            QueueRecords.Clear();
            foreach (var r in _examService.GetQueueForDoctor(_doctorId)) QueueRecords.Add(r);

            Services.Clear();
            foreach (var s in _examService.GetServices()) Services.Add(s);

            Drugs.Clear();
            foreach (var d in _examService.GetDrugs()) Drugs.Add(d);

            Rooms.Clear();
            foreach (var rm in _examService.GetRooms()) Rooms.Add(rm);

            MyShifts.Clear();
            foreach (var sh in _examService.GetShifts(_doctorId)) MyShifts.Add(sh);

            Questions.Clear();
            foreach (var q in _examService.GetDoctorQuestions(_doctorId)) Questions.Add(q);

            if (selectedId.HasValue)
            {
                foreach (var r in QueueRecords)
                {
                    if (r.RecordId == selectedId.Value)
                    {
                        SelectedRecord = r;
                        break;
                    }
                }
            }
        }

        private void PopulateSelectedRecord()
        {
            CurrentRecordServices.Clear();
            CurrentRecordPrescriptions.Clear();
            if (SelectedRecord == null) return;

            Symptoms = SelectedRecord.Symptoms ?? string.Empty;
            Diagnosis = SelectedRecord.Diagnosis ?? string.Empty;
            IsInpatient = SelectedRecord.IsInpatient ?? false;

            var m = SelectedRecord.Exammetric;
            VisionRight = m?.VisionRight;
            VisionLeft = m?.VisionLeft;
            IopRight = m?.IopRight;
            IopLeft = m?.IopLeft;
            SphRight = m?.SphRight;
            CylRight = m?.CylRight;
            AxisRight = m?.AxisRight;
            SphLeft = m?.SphLeft;
            CylLeft = m?.CylLeft;
            AxisLeft = m?.AxisLeft;

            foreach (var s in SelectedRecord.Servicedetails) CurrentRecordServices.Add(s);
            foreach (var p in SelectedRecord.Prescriptiondetails) CurrentRecordPrescriptions.Add(p);
        }

        private void SignRefraction()
        {
            if (SelectedRecord == null) return;
            _examService.SaveExamMetrics(SelectedRecord.RecordId, null, new Exammetric
            {
                VisionRight = VisionRight,
                VisionLeft = VisionLeft,
                IopRight = IopRight,
                IopLeft = IopLeft,
                SphRight = SphRight,
                CylRight = CylRight,
                AxisRight = AxisRight,
                SphLeft = SphLeft,
                CylLeft = CylLeft,
                AxisLeft = AxisLeft
            });
            StatusMessage = $"Đã ký xác nhận đơn kính cho hồ sơ #{SelectedRecord.RecordId}.";
            LoadAll();
        }

        private void OrderService()
        {
            if (SelectedRecord == null || SelectedService == null) return;
            _examService.OrderService(SelectedRecord.RecordId, SelectedService.ServiceId);
            StatusMessage = $"Đã chỉ định dịch vụ {SelectedService.ServiceName}.";
            LoadAll();
        }

        private void AddDrug()
        {
            if (SelectedRecord == null || SelectedDrug == null || DrugQty <= 0) return;
            _examService.AddPrescriptionItem(SelectedRecord.RecordId, SelectedDrug.DrugId, DrugQty, DrugDosage);
            StatusMessage = $"Đã thêm thuốc {SelectedDrug.DrugName} vào đơn.";
            LoadAll();
        }

        private void AddCareOrder()
        {
            if (SelectedRecord == null || string.IsNullOrWhiteSpace(CareActionName)) return;
            _examService.CreateCareOrder(SelectedRecord.RecordId, null, CareActionName.Trim());
            StatusMessage = $"Đã tạo y lệnh điều dưỡng: {CareActionName}.";
        }

        private void FinishExam()
        {
            if (SelectedRecord == null) return;
            bool inpatient = IsInpatient || ExamOutcome == "Chỉ định nhập viện";
            _examService.CreateDrugBillForRecord(SelectedRecord.RecordId);
            _examService.SaveClinicalExamination(SelectedRecord.RecordId, Symptoms, Diagnosis, inpatient, ExamOutcome);
            StatusMessage = $"Đã hoàn tất hồ sơ #{SelectedRecord.RecordId} ({ExamOutcome}).";
            SelectedRecord = null;
            LoadAll();
        }

        private void RegisterShift()
        {
            if (SelectedRoom == null) return;
            _examService.RegisterShift(_doctorId, SelectedRoom.RoomId, ShiftDate, ShiftTimeFrame);
            StatusMessage = "Đã đăng ký lịch trực thành công.";
            LoadAll();
        }

        private void ToggleRoomStatus()
        {
            if (SelectedRoom == null) return;
            string nextStatus = SelectedRoom.Status == "Trống" ? "Đang sử dụng" : "Trống";
            _examService.UpdateRoomStatus(SelectedRoom.RoomId, nextStatus);
            LoadAll();
        }

        private void ReplyQuestion()
        {
            if (SelectedQuestion == null || string.IsNullOrWhiteSpace(AnswerText)) return;
            _examService.AnswerQuestion(SelectedQuestion.QuestionId, _doctorId, AnswerText.Trim());
            StatusMessage = "Đã gửi câu trả lời cho bệnh nhân.";
            LoadAll();
        }
    }
}