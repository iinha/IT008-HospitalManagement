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
            using var db = new HospitalDbContext();
            return db.Patients.FirstOrDefault(p => p.PatientId == patientId);
        }

        public void UpdateProfile(Patient updated)
        {
            using var db = new HospitalDbContext();
            var existing = db.Patients.FirstOrDefault(p => p.PatientId == updated.PatientId);
            if (existing == null) return;

            existing.Fullname = updated.Fullname;
            existing.Phone = updated.Phone;
            existing.DateofBirth = updated.DateofBirth;
            existing.Gender = updated.Gender;
            existing.HealthInsuranceCode = updated.HealthInsuranceCode;
            db.SaveChanges();
        }

        public List<User> GetDoctors()
        {
            using var db = new HospitalDbContext();
            return db.Users
                .Where(u => u.Role == "Doctor" && u.IsActive != false)
                .OrderBy(u => u.Fullname)
                .ToList();
        }

        public Appointment BookAppointment(int patientId, int doctorId, DateTime date, string timeSlot)
        {
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
            return appt;
        }

        public List<Appointment> GetAppointmentsByPatient(int patientId)
        {
            using var db = new HospitalDbContext();
            return db.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.ApptDate)
                .ToList();
        }

        public List<Medicalrecord> GetMedicalHistory(int patientId)
        {
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

        public void SubmitQuestion(int patientId, int? doctorId, string content)
        {
            using var db = new HospitalDbContext();
            int nextId = (db.Questions.Max(q => (int?)q.QuestionId) ?? 0) + 1;
            db.Questions.Add(new Question
            {
                QuestionId = nextId,
                PatientId = patientId,
                DoctorId = doctorId,
                Content = content,
                Status = "Chờ trả lời",
                CreatedAt = DateTime.Now
            });
            db.SaveChanges();
        }
    }
}