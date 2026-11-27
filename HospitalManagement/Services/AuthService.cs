using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using HospitalManagement.Data;
using HospitalManagement.Models;

namespace HospitalManagement.Services
{
    public class AuthSession
    {
        public int Id { get; set; }
        public string Fullname { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsPatient { get; set; }
    }

    public class AuthService
    {
        public static AuthSession? CurrentSession { get; private set; }

        public string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }

        private bool VerifyPassword(string inputPassword, string? storedHash)
        {
            if (string.IsNullOrEmpty(storedHash)) return false;
            return storedHash == inputPassword || storedHash.Equals(HashPassword(inputPassword), StringComparison.OrdinalIgnoreCase);
        }

        public AuthSession? LoginStaff(string username, string password)
        {
            using var db = new HospitalDbContext();
            var user = db.Users.FirstOrDefault(u => u.Username == username);
            if (user == null || user.IsActive == false || !VerifyPassword(password, user.PasswordHash))
                return null;

            CurrentSession = new AuthSession
            {
                Id = user.UserId,
                Fullname = user.Fullname ?? user.Username ?? string.Empty,
                Role = user.Role ?? string.Empty,
                IsPatient = false
            };

            WriteLog(db, user.UserId, $"Đăng nhập hệ thống ({user.Role})");
            return CurrentSession;
        }

        public AuthSession? LoginPatient(string phone, string password)
        {
            using var db = new HospitalDbContext();
            var patient = db.Patients.FirstOrDefault(p => p.Phone == phone);
            if (patient == null || !VerifyPassword(password, patient.PasswordHash))
                return null;

            CurrentSession = new AuthSession
            {
                Id = patient.PatientId,
                Fullname = patient.Fullname ?? patient.Phone ?? string.Empty,
                Role = "Patient",
                IsPatient = true
            };
            return CurrentSession;
        }

        public bool RegisterPatient(string fullname, string phone, string password, DateTime? dob, string gender, string bhyt, out string error)
        {
            error = string.Empty;
            using var db = new HospitalDbContext();
            if (db.Patients.Any(p => p.Phone == phone))
            {
                error = "Số điện thoại đã được đăng ký.";
                return false;
            }

            int nextId = (db.Patients.Max(p => (int?)p.PatientId) ?? 0) + 1;
            var patient = new Patient
            {
                PatientId = nextId,
                Fullname = fullname,
                Phone = phone,
                PasswordHash = HashPassword(password),
                DateofBirth = dob,
                Gender = gender,
                HealthInsuranceCode = bhyt
            };

            db.Patients.Add(patient);
            db.SaveChanges();
            return true;
        }

        public void Logout()
        {
            if (CurrentSession != null && !CurrentSession.IsPatient)
            {
                using var db = new HospitalDbContext();
                WriteLog(db, CurrentSession.Id, "Đăng xuất hệ thống");
            }
            CurrentSession = null;
        }

        public void WriteLog(HospitalDbContext db, int userId, string action)
        {
            int nextLogId = (db.Syslogs.Max(l => (int?)l.LogId) ?? 0) + 1;
            db.Syslogs.Add(new Syslog
            {
                LogId = nextLogId,
                UserId = userId,
                Action = action,
                LoggedAt = DateTime.Now
            });
            db.SaveChanges();
        }
    }
}