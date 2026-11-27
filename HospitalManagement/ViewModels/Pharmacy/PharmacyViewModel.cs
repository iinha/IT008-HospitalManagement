using System.Collections.ObjectModel;
using System.Windows.Input;
using HospitalManagement.Helpers;
using HospitalManagement.Models;
using HospitalManagement.Services;

namespace HospitalManagement.ViewModels.Pharmacy
{
    public class PharmacyViewModel : BaseViewModel
    {
        private readonly BillingService _billingService = new();
        private Medicalrecord? _selectedRecord;
        private string _statusMessage = string.Empty;

        public ObservableCollection<Medicalrecord> PrescriptionRecords { get; } = new();
        public ObservableCollection<Prescriptiondetail> SelectedItems { get; } = new();

        public Medicalrecord? SelectedRecord
        {
            get => _selectedRecord;
            set
            {
                if (SetProperty(ref _selectedRecord, value))
                {
                    SelectedItems.Clear();
                    if (value != null)
                    {
                        foreach (var item in value.Prescriptiondetails) SelectedItems.Add(item);
                    }
                }
            }
        }

        public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }

        public ICommand RefreshCommand { get; }
        public ICommand DispenseCommand { get; }

        public PharmacyViewModel()
        {
            RefreshCommand = new RelayCommand(_ => LoadData());
            DispenseCommand = new RelayCommand(_ => Dispense());
            LoadData();
        }

        public void LoadData()
        {
            PrescriptionRecords.Clear();
            foreach (var r in _billingService.GetPendingPrescriptions()) PrescriptionRecords.Add(r);
        }

        private void Dispense()
        {
            if (SelectedRecord == null) return;
            _billingService.DispensePrescription(SelectedRecord.RecordId, out string msg);
            StatusMessage = msg;
            LoadData();
        }
    }
}