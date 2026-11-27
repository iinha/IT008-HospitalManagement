using System;
using System.Collections.Generic;
using System.Linq;
using HospitalManagement.Data;
using HospitalManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Services
{
    public class PatientService
    {
        public Patient? GetPatientById(int patientId)
        {
            if (patientId <= 0)
            {
                return new Patient
                {
                    PatientId = patientId,
                    Fullname = "Bệnh nhân mẫu",
                    Phone = "0901234567",
                    DateofBirth = new DateTime(2000, 1, 1),
                    Gender = "Nam",
                    HealthInsuranceCode = "DN4790000000000"
                };
            }

            using var db = new HospitalDbContext();
            return db.Patients.FirstOrDefault(p => p.PatientId == patientId);
        }

        public bool UpdateProfile(Patient updated, out string message)
        {
            if (updated.PatientId <= 0)
            {
                message = "Tài khoản test mặc định chưa có trong CSDL nên không thể lưu. Hãy đăng ký tài khoản bệnh nhân mới.";
                return false;
            }

            using var db = new HospitalDbContext();
            var existing = db.Patients.FirstOrDefault(p => p.PatientId == updated.PatientId);
            if (existing == null)
            {
                message = "Không tìm thấy hồ sơ bệnh nhân trong CSDL.";
                return false;
            }

            if (db.Patients.Any(p => p.Phone == updated.Phone && p.PatientId != updated.PatientId))
            {
                message = "Số điện thoại đã thuộc về tài khoản khác.";
                return false;
            }

            existing.Fullname = updated.Fullname?.Trim();
            existing.Phone = updated.Phone?.Trim();
            existing.DateofBirth = updated.DateofBirth;
            existing.Gender = updated.Gender?.Trim();
            existing.HealthInsuranceCode = updated.HealthInsuranceCode?.Trim();
            db.SaveChanges();
            message = "Đã lưu cập nhật hồ sơ vào CSDL thành công.";
            return true;
        }

        public List<User> GetDoctors()
        {
            using var db = new HospitalDbContext();
            return db.Users
                .Where(u => u.Role == "Doctor" && u.IsActive != false)
                .OrderBy(u => u.Fullname)
                .ToList();
        }

        public Appointment? BookAppointment(int patientId, int doctorId, DateTime date, string timeSlot, out string message)
        {
            if (patientId <= 0)
            {
                message = "Tài khoản test mặc định không thể lưu lịch hẹn vào CSDL. Hãy đăng ký tài khoản bệnh nhân thật.";
                return null;
            }

            using var db = new HospitalDbContext();
            int nextId = (db.Appointments.Max(a => (int?)a.ApptId) ?? 0) + 1;
            var appt = new Appointment
            {
                ApptId = nextId,
                PatientId = patientId,
                DoctorId = doctorId,
                ApptDate = DateOnly.FromDateTime(date),
                TimeSlot = timeSlot,
                Status = "Đã đặt lịch"
            };
            db.Appointments.Add(appt);
            db.SaveChanges();
            message = $"Đặt lịch thành công. Mã đặt hẹn của bạn: APT-{appt.ApptId:D5}";
            return appt;
        }

        public List<Appointment> GetAppointmentsByPatient(int patientId)
        {
            if (patientId <= 0) return new List<Appointment>();
            using var db = new HospitalDbContext();
            return db.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.ApptDate)
                .ToList();
        }

        public List<Medicalrecord> GetMedicalHistory(int patientId)
        {
            if (patientId <= 0) return new List<Medicalrecord>();
            using var db = new HospitalDbContext();
            return db.Medicalrecords
                .Include(m => m.Doctor)
                .Include(m => m.Exammetric)
                .Include(m => m.Prescriptiondetails).ThenInclude(p => p.Drug)
                .Include(m => m.Bills)
                .Where(m => m.PatientId == patientId)
                .OrderByDescending(m => m.VisitDate)
                .ToList();
        }

        public List<Bill> GetBillsByPatient(int patientId)
        {
            if (patientId <= 0) return new List<Bill>();
            using var db = new HospitalDbContext();
            return db.Bills
                .Include(b => b.Record)
                .Where(b => b.Record != null && b.Record.PatientId == patientId)
                .OrderByDescending(b => b.CreatedAt)
                .ToList();
        }

        public List<Article> GetArticles(string? category = null)
        {
            using var db = new HospitalDbContext();
            var query = db.Articles.Include(a => a.Author).AsQueryable();
            if (!string.IsNullOrWhiteSpace(category) && category != "Tất cả")
                query = query.Where(a => a.Category == category);
            return query.OrderByDescending(a => a.CreatedAt).ToList();
        }

        public List<Question> GetQuestionsByPatient(int patientId)
        {
            if (patientId <= 0) return new List<Question>();
            using var db = new HospitalDbContext();
            return db.Questions
                .Include(q => q.Doctor)
                .Where(q => q.PatientId == patientId)
                .OrderByDescending(q => q.CreatedAt)
                .ToList();
        }

        public List<Question> GetAllPublicQuestions()
        {
            using var db = new HospitalDbContext();
            return db.Questions
                .Include(q => q.Patient)
                .Include(q => q.Doctor)
                .OrderByDescending(q => q.CreatedAt)
                .ToList();
        }

        public bool SubmitQuestion(int patientId, int? doctorId, string content, out string message)
        {
            if (patientId <= 0)
            {
                message = "Tài khoản test mặc định không thể gửi câu hỏi vào CSDL. Hãy đăng ký tài khoản thật.";
                return false;
            }

            using var db = new HospitalDbContext();
            int nextId = (db.Questions.Max(q => (int?)q.QuestionId) ?? 0) + 1;
            db.Questions.Add(new Question
            {
                QuestionId = nextId,
                PatientId = patientId,
                DoctorId = doctorId.HasValue && doctorId.Value > 0 ? doctorId.Value : null,
                Content = content,
                Status = "Chờ trả lời",
                CreatedAt = DateTime.Now
            });
            db.SaveChanges();
            message = "Đã gửi câu hỏi thành công.";
            return true;
        }
    }
}