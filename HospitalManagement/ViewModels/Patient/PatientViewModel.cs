using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using HospitalManagement.Helpers;
using HospitalManagement.Models;
using HospitalManagement.Services;

namespace HospitalManagement.ViewModels.Patient
{
    public class PatientViewModel : BaseViewModel
    {
        private readonly PatientService _patientService = new();
        private readonly int _patientId;

        private Models.Patient? _profile;
        private User? _selectedDoctor;
        private DateTime _bookingDate = DateTime.Today.AddDays(1);
        private string _selectedTimeSlot = "08:00 - 09:00";
        private string _bookingCode = string.Empty;

        private string _selectedCategory = "Tất cả";
        private Article? _selectedArticle;

        private User? _questionDoctor;
        private string _questionContent = string.Empty;
        private string _statusMessage = string.Empty;

        public ObservableCollection<User> Doctors { get; } = new();
        public ObservableCollection<Appointment> MyAppointments { get; } = new();
        public ObservableCollection<Medicalrecord> MedicalHistory { get; } = new();
        public ObservableCollection<Bill> MyBills { get; } = new();
        public ObservableCollection<Article> Articles { get; } = new();
        public ObservableCollection<Question> MyQuestions { get; } = new();

        public string[] TimeSlots { get; } = { "08:00 - 09:00", "09:00 - 10:00", "10:00 - 11:00", "13:30 - 14:30", "14:30 - 15:30", "15:30 - 16:30" };
        public string[] Categories { get; } = { "Tất cả", "Giới thiệu", "Cơ sở vật chất", "Đội ngũ bác sĩ", "Tin tức sức khỏe" };

        public Models.Patient? Profile { get => _profile; set => SetProperty(ref _profile, value); }
        public User? SelectedDoctor { get => _selectedDoctor; set => SetProperty(ref _selectedDoctor, value); }
        public DateTime BookingDate { get => _bookingDate; set => SetProperty(ref _bookingDate, value); }
        public string SelectedTimeSlot { get => _selectedTimeSlot; set => SetProperty(ref _selectedTimeSlot, value); }
        public string BookingCode { get => _bookingCode; set => SetProperty(ref _bookingCode, value); }

        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value)) LoadArticles();
            }
        }

        public Article? SelectedArticle { get => _selectedArticle; set => SetProperty(ref _selectedArticle, value); }
        public User? QuestionDoctor { get => _questionDoctor; set => SetProperty(ref _questionDoctor, value); }
        public string QuestionContent { get => _questionContent; set => SetProperty(ref _questionContent, value); }
        public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }

        public ICommand SaveProfileCommand { get; }
        public ICommand BookCommand { get; }
        public ICommand SendQuestionCommand { get; }
        public ICommand RefreshCommand { get; }

        public PatientViewModel(int patientId)
        {
            _patientId = patientId;
            SaveProfileCommand = new RelayCommand(_ => SaveProfile());
            BookCommand = new RelayCommand(_ => BookAppointment());
            SendQuestionCommand = new RelayCommand(_ => SendQuestion());
            RefreshCommand = new RelayCommand(_ => LoadAll());
            LoadAll();
        }

        private void LoadAll()
        {
            Profile = _patientService.GetPatientById(_patientId);

            Doctors.Clear();
            foreach (var d in _patientService.GetDoctors()) Doctors.Add(d);

            MyAppointments.Clear();
            foreach (var a in _patientService.GetAppointmentsByPatient(_patientId)) MyAppointments.Add(a);

            MedicalHistory.Clear();
            foreach (var m in _patientService.GetMedicalHistory(_patientId)) MedicalHistory.Add(m);

            MyBills.Clear();
            foreach (var b in _patientService.GetBillsByPatient(_patientId)) MyBills.Add(b);

            MyQuestions.Clear();
            foreach (var q in _patientService.GetQuestionsByPatient(_patientId)) MyQuestions.Add(q);

            LoadArticles();
        }

        private void LoadArticles()
        {
            Articles.Clear();
            foreach (var art in _patientService.GetArticles(SelectedCategory)) Articles.Add(art);
        }

        private void SaveProfile()
        {
            if (Profile == null) return;
            _patientService.UpdateProfile(Profile);
            StatusMessage = "Đã cập nhật hồ sơ cá nhân thành công.";
        }

        private void BookAppointment()
        {
            if (SelectedDoctor == null)
            {
                StatusMessage = "Vui lòng chọn bác sĩ khám.";
                return;
            }
            var appt = _patientService.BookAppointment(_patientId, SelectedDoctor.UserId, BookingDate, SelectedTimeSlot);
            BookingCode = $"APT-{appt.ApptId:D5}";
            StatusMessage = $"Đặt lịch thành công. Mã đặt hẹn của bạn: {BookingCode}";
            LoadAll();
        }

        private void SendQuestion()
        {
            if (string.IsNullOrWhiteSpace(QuestionContent)) return;
            _patientService.SubmitQuestion(_patientId, QuestionDoctor?.UserId, QuestionContent.Trim());
            QuestionContent = string.Empty;
            StatusMessage = "Đã gửi câu hỏi đến bác sĩ.";
            LoadAll();
        }
    }
}