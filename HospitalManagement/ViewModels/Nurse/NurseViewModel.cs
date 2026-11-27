using System.Collections.ObjectModel;
using System.Windows.Input;
using HospitalManagement.Helpers;
using HospitalManagement.Models;
using HospitalManagement.Services;

namespace HospitalManagement.ViewModels.Nurse
{
    public class NurseViewModel : BaseViewModel
    {
        private readonly ExaminationService _examService = new();
        private readonly int _nurseId;

        private Medicalrecord? _selectedRecord;
        private Careorder? _selectedCareOrder;
        private Servicedetail? _selectedServiceOrder;

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
        private string _resultNote = string.Empty;
        private string _statusMessage = string.Empty;

        public ObservableCollection<Medicalrecord> WaitingRecords { get; } = new();
        public ObservableCollection<Servicedetail> ServiceOrders { get; } = new();
        public ObservableCollection<Careorder> CareOrders { get; } = new();

        public Medicalrecord? SelectedRecord
        {
            get => _selectedRecord;
            set
            {
                if (SetProperty(ref _selectedRecord, value) && value != null)
                {
                    var m = value.Exammetric;
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
                }
            }
        }

        public Careorder? SelectedCareOrder { get => _selectedCareOrder; set => SetProperty(ref _selectedCareOrder, value); }
        public Servicedetail? SelectedServiceOrder
        {
            get => _selectedServiceOrder;
            set
            {
                if (SetProperty(ref _selectedServiceOrder, value) && value != null)
                    ResultNote = value.ResultNote ?? string.Empty;
            }
        }

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
        public string ResultNote { get => _resultNote; set => SetProperty(ref _resultNote, value); }
        public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }

        public ICommand RefreshCommand { get; }
        public ICommand SaveMetricsCommand { get; }
        public ICommand CompleteCareOrderCommand { get; }
        public ICommand SaveServiceResultCommand { get; }

        public NurseViewModel(int nurseId)
        {
            _nurseId = nurseId;
            RefreshCommand = new RelayCommand(_ => LoadAll());
            SaveMetricsCommand = new RelayCommand(_ => SaveMetrics());
            CompleteCareOrderCommand = new RelayCommand(_ => CompleteCareOrder());
            SaveServiceResultCommand = new RelayCommand(_ => SaveServiceResult());
            LoadAll();
        }

        public void LoadAll()
        {
            WaitingRecords.Clear();
            foreach (var r in _examService.GetRecordsForNurse()) WaitingRecords.Add(r);

            ServiceOrders.Clear();
            foreach (var s in _examService.GetAllServiceOrders()) ServiceOrders.Add(s);

            CareOrders.Clear();
            foreach (var c in _examService.GetCareOrders()) CareOrders.Add(c);
        }

        private void SaveMetrics()
        {
            if (SelectedRecord == null) return;
            _examService.SaveExamMetrics(SelectedRecord.RecordId, _nurseId, new Exammetric
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
            StatusMessage = $"Đã lưu chỉ số đo cho hồ sơ #{SelectedRecord.RecordId}.";
            LoadAll();
        }

        private void CompleteCareOrder()
        {
            if (SelectedCareOrder == null) return;
            _examService.CompleteCareOrder(SelectedCareOrder.CareOrderId, _nurseId);
            StatusMessage = $"Đã hoàn thành y lệnh #{SelectedCareOrder.CareOrderId}.";
            LoadAll();
        }

        private void SaveServiceResult()
        {
            if (SelectedServiceOrder == null) return;
            _examService.UpdateServiceResult(SelectedServiceOrder.RecordId, SelectedServiceOrder.ServiceId, ResultNote);
            StatusMessage = "Đã cập nhật kết quả cận lâm sàng.";
            LoadAll();
        }
    }
}