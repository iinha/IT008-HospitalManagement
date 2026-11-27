using System;
using System.Collections.Generic;
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

        private static readonly List<(string Username, string Password, AuthSession Session)> DefaultStaffAccounts = new()
        {
            ("admin", "123456", new AuthSession { Id = -1, Fullname = "Quản trị hệ thống", Role = "Admin", IsPatient = false }),
            ("manager", "123456", new AuthSession { Id = -2, Fullname = "Ban quản lý", Role = "Management", IsPatient = false }),
            ("doctor", "123456", new AuthSession { Id = -3, Fullname = "BS. Nguyễn Lạc Thường Miên", Role = "Doctor", IsPatient = false }),
            ("nurse", "123456", new AuthSession { Id = -4, Fullname = "ĐD. Nguyễn Võ Ái Nhã", Role = "Nurse", IsPatient = false }),
            ("reception", "123456", new AuthSession { Id = -5, Fullname = "LT. Võ Duy Hoàng", Role = "Reception", IsPatient = false }),
            ("pharmacy", "123456", new AuthSession { Id = -6, Fullname = "DS. Nguyễn Văn A", Role = "Pharmacy", IsPatient = false })
        };

        private static readonly List<(string Phone, string Password, AuthSession Session)> DefaultPatientAccounts = new()
        {
            ("0901234567", "123456", new AuthSession { Id = -100, Fullname = "Bệnh nhân mẫu", Role = "Patient", IsPatient = true })
        };

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
            if (user != null)
            {
                if (user.IsActive == false || !VerifyPassword(password, user.PasswordHash))
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

            var defaultAcc = DefaultStaffAccounts.FirstOrDefault(a =>
                a.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && a.Password == password);
            if (defaultAcc.Session != null)
            {
                CurrentSession = defaultAcc.Session;
                return CurrentSession;
            }

            return null;
        }

        public AuthSession? LoginPatient(string phone, string password)
        {
            using var db = new HospitalDbContext();
            var patient = db.Patients.FirstOrDefault(p => p.Phone == phone);
            if (patient != null)
            {
                if (!VerifyPassword(password, patient.PasswordHash))
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

            var defaultAcc = DefaultPatientAccounts.FirstOrDefault(a =>
                a.Phone == phone && a.Password == password);
            if (defaultAcc.Session != null)
            {
                CurrentSession = defaultAcc.Session;
                return CurrentSession;
            }

            return null;
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
            if (CurrentSession != null && !CurrentSession.IsPatient && CurrentSession.Id > 0)
            {
                using var db = new HospitalDbContext();
                WriteLog(db, CurrentSession.Id, "Đăng xuất hệ thống");
            }
            CurrentSession = null;
        }

        public void WriteLog(HospitalDbContext db, int userId, string action)
        {
            if (userId <= 0 || !db.Users.Any(u => u.UserId == userId)) return;
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