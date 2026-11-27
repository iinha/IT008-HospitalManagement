using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
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

        private string _bookingStatusMessage = string.Empty;
        private string _profileStatusMessage = string.Empty;
        private string _qnaStatusMessage = string.Empty;

        public ObservableCollection<User> Doctors { get; } = new();
        public ObservableCollection<User> QuestionDoctors { get; } = new();
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

        public string BookingStatusMessage { get => _bookingStatusMessage; set => SetProperty(ref _bookingStatusMessage, value); }
        public string ProfileStatusMessage { get => _profileStatusMessage; set => SetProperty(ref _profileStatusMessage, value); }
        public string QnaStatusMessage { get => _qnaStatusMessage; set => SetProperty(ref _qnaStatusMessage, value); }

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

        public void LoadAll()
        {
            BookingStatusMessage = string.Empty;
            ProfileStatusMessage = string.Empty;
            QnaStatusMessage = string.Empty;

            Profile = _patientService.GetPatientById(_patientId);

            var doctorList = _patientService.GetDoctors();
            Doctors.Clear();
            QuestionDoctors.Clear();
            var noneOption = new User { UserId = 0, Fullname = "Không" };
            QuestionDoctors.Add(noneOption);

            foreach (var d in doctorList)
            {
                Doctors.Add(d);
                QuestionDoctors.Add(d);
            }
            QuestionDoctor = noneOption;

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
            if (string.IsNullOrWhiteSpace(Profile.Fullname))
            {
                ProfileStatusMessage = "Họ và tên không được để trống.";
                return;
            }
            if (string.IsNullOrWhiteSpace(Profile.Phone) || !Regex.IsMatch(Profile.Phone.Trim(), @"^\d{9,11}$"))
            {
                ProfileStatusMessage = "Số điện thoại phải gồm 9-11 chữ số.";
                return;
            }
            _patientService.UpdateProfile(Profile, out string msg);
            ProfileStatusMessage = msg;
        }

        private void BookAppointment()
        {
            if (SelectedDoctor == null)
            {
                BookingStatusMessage = "Vui lòng chọn bác sĩ khám.";
                return;
            }
            var appt = _patientService.BookAppointment(_patientId, SelectedDoctor.UserId, BookingDate, SelectedTimeSlot, out string msg);
            if (appt != null)
            {
                BookingCode = $"APT-{appt.ApptId:D5}";
                LoadAll();
            }
            BookingStatusMessage = msg;
        }

        private void SendQuestion()
        {
            if (string.IsNullOrWhiteSpace(QuestionContent))
            {
                QnaStatusMessage = "Vui lòng nhập nội dung câu hỏi.";
                return;
            }
            int? docId = QuestionDoctor != null && QuestionDoctor.UserId > 0 ? QuestionDoctor.UserId : null;
            if (_patientService.SubmitQuestion(_patientId, docId, QuestionContent.Trim(), out string msg))
            {
                QuestionContent = string.Empty;
                LoadAll();
            }
            QnaStatusMessage = msg;
        }
    }
}