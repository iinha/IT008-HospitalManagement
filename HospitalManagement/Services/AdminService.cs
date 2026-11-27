using System;
using System.Collections.Generic;
using System.Linq;
using HospitalManagement.Data;
using HospitalManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Services
{
    public class DoctorPerformanceDto
    {
        public string DoctorName { get; set; } = string.Empty;
        public int ExamCount { get; set; }
    }

    public class RevenueByDateDto
    {
        public string DateLabel { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
    }

    public class AdminService
    {
        private readonly AuthService _authService = new();

        public List<User> GetAllUsers()
        {
            using var db = new HospitalDbContext();
            return db.Users.OrderBy(u => u.UserId).ToList();
        }

        public bool CreateUser(string username, string password, string fullname, string role, out string message)
        {
            using var db = new HospitalDbContext();
            if (db.Users.Any(u => u.Username == username))
            {
                message = "Tên đăng nhập đã tồn tại.";
                return false;
            }

            int nextId = (db.Users.Max(u => (int?)u.UserId) ?? 0) + 1;
            db.Users.Add(new User
            {
                UserId = nextId,
                Username = username,
                PasswordHash = _authService.HashPassword(password),
                Fullname = fullname,
                Role = role,
                IsActive = true
            });
            db.SaveChanges();
            LogCurrentUser(db, $"Tạo tài khoản {username} ({role})");
            message = "Đã tạo tài khoản thành công.";
            return true;
        }

        public bool UpdateUser(User user)
        {
            using var db = new HospitalDbContext();
            var existing = db.Users.FirstOrDefault(u => u.UserId == user.UserId);
            if (existing == null) return false;
            existing.Fullname = user.Fullname;
            existing.Role = user.Role;
            existing.IsActive = user.IsActive;
            db.SaveChanges();
            LogCurrentUser(db, $"Cập nhật nhân sự ID={user.UserId}");
            return true;
        }

        public bool ToggleUserLock(int userId)
        {
            using var db = new HospitalDbContext();
            var existing = db.Users.FirstOrDefault(u => u.UserId == userId);
            if (existing == null) return false;
            existing.IsActive = !(existing.IsActive ?? true);
            db.SaveChanges();
            LogCurrentUser(db, $"Thay đổi trạng thái tài khoản ID={userId} thành {(existing.IsActive == true ? "Mở" : "Khóa")}");
            return true;
        }

        public bool ResetPassword(int userId, string newPassword)
        {
            using var db = new HospitalDbContext();
            var existing = db.Users.FirstOrDefault(u => u.UserId == userId);
            if (existing == null) return false;
            existing.PasswordHash = _authService.HashPassword(newPassword);
            db.SaveChanges();
            LogCurrentUser(db, $"Cấp lại mật khẩu cho tài khoản ID={userId}");
            return true;
        }

        public void DeleteUser(int userId)
        {
            using var db = new HospitalDbContext();
            var existing = db.Users.FirstOrDefault(u => u.UserId == userId);
            if (existing == null) return;
            db.Users.Remove(existing);
            db.SaveChanges();
            LogCurrentUser(db, $"Xóa nhân sự ID={userId}");
        }

        public List<Patient> GetAllPatients()
        {
            using var db = new HospitalDbContext();
            return db.Patients.OrderBy(p => p.PatientId).ToList();
        }

        public List<Drug> GetAllDrugs()
        {
            using var db = new HospitalDbContext();
            return db.Drugs.OrderBy(d => d.DrugId).ToList();
        }

        public void SaveDrug(Drug drug)
        {
            using var db = new HospitalDbContext();
            if (drug.DrugId == 0)
            {
                drug.DrugId = (db.Drugs.Max(d => (int?)d.DrugId) ?? 0) + 1;
                db.Drugs.Add(drug);
            }
            else
            {
                var existing = db.Drugs.FirstOrDefault(d => d.DrugId == drug.DrugId);
                if (existing != null)
                {
                    existing.DrugName = drug.DrugName;
                    existing.Unit = drug.Unit;
                    existing.UnitPrice = drug.UnitPrice;
                    existing.StockQuantity = drug.StockQuantity;
                }
            }
            db.SaveChanges();
        }

        public void DeleteDrug(int drugId)
        {
            using var db = new HospitalDbContext();
            var existing = db.Drugs.FirstOrDefault(d => d.DrugId == drugId);
            if (existing == null) return;
            db.Drugs.Remove(existing);
            db.SaveChanges();
        }

        public List<Service> GetAllServices()
        {
            using var db = new HospitalDbContext();
            return db.Services.OrderBy(s => s.ServiceId).ToList();
        }

        public void SaveService(Service service)
        {
            using var db = new HospitalDbContext();
            if (service.ServiceId == 0)
            {
                service.ServiceId = (db.Services.Max(s => (int?)s.ServiceId) ?? 0) + 1;
                db.Services.Add(service);
            }
            else
            {
                var existing = db.Services.FirstOrDefault(s => s.ServiceId == service.ServiceId);
                if (existing != null)
                {
                    existing.ServiceName = service.ServiceName;
                    existing.Price = service.Price;
                    existing.Category = service.Category;
                }
            }
            db.SaveChanges();
        }

        public void DeleteService(int serviceId)
        {
            using var db = new HospitalDbContext();
            var existing = db.Services.FirstOrDefault(s => s.ServiceId == serviceId);
            if (existing == null) return;
            db.Services.Remove(existing);
            db.SaveChanges();
        }

        public List<Room> GetAllRooms()
        {
            using var db = new HospitalDbContext();
            return db.Rooms.OrderBy(r => r.RoomId).ToList();
        }

        public void SaveRoom(Room room)
        {
            using var db = new HospitalDbContext();
            if (room.RoomId == 0)
            {
                room.RoomId = (db.Rooms.Max(r => (int?)r.RoomId) ?? 0) + 1;
                db.Rooms.Add(room);
            }
            else
            {
                var existing = db.Rooms.FirstOrDefault(r => r.RoomId == room.RoomId);
                if (existing != null)
                {
                    existing.RoomName = room.RoomName;
                    existing.Status = room.Status;
                }
            }
            db.SaveChanges();
        }

        public List<Article> GetAllArticles()
        {
            using var db = new HospitalDbContext();
            return db.Articles.Include(a => a.Author).OrderByDescending(a => a.CreatedAt).ToList();
        }

        public void SaveArticle(Article article)
        {
            using var db = new HospitalDbContext();
            if (article.ArticleId == 0)
            {
                article.ArticleId = (db.Articles.Max(a => (int?)a.ArticleId) ?? 0) + 1;
                article.CreatedAt = DateTime.Now;
                article.AuthorId = AuthService.CurrentSession != null && AuthService.CurrentSession.Id > 0
                    ? AuthService.CurrentSession.Id
                    : null;
                db.Articles.Add(article);
            }
            else
            {
                var existing = db.Articles.FirstOrDefault(a => a.ArticleId == article.ArticleId);
                if (existing != null)
                {
                    existing.Title = article.Title;
                    existing.Category = article.Category;
                    existing.Content = article.Content;
                }
            }
            db.SaveChanges();
        }

        public void DeleteArticle(int articleId)
        {
            using var db = new HospitalDbContext();
            var existing = db.Articles.FirstOrDefault(a => a.ArticleId == articleId);
            if (existing == null) return;
            db.Articles.Remove(existing);
            db.SaveChanges();
        }

        public List<Syslog> GetSysLogs()
        {
            using var db = new HospitalDbContext();
            return db.Syslogs.Include(l => l.User).OrderByDescending(l => l.LoggedAt).ToList();
        }

        public void BackupDatabase(string filePath)
        {
            using var db = new HospitalDbContext();
            string safePath = filePath.Replace("'", "''");
            db.Database.ExecuteSqlRaw($"BACKUP DATABASE [G5_QLBV] TO DISK = N'{safePath}' WITH FORMAT, INIT");
            LogCurrentUser(db, $"Sao lưu CSDL tới {filePath}");
        }

        public void RestoreDatabase(string filePath)
        {
            using var db = new HospitalDbContext();
            string safePath = filePath.Replace("'", "''");
            db.Database.ExecuteSqlRaw($"USE master; ALTER DATABASE [G5_QLBV] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; RESTORE DATABASE [G5_QLBV] FROM DISK = N'{safePath}' WITH REPLACE; ALTER DATABASE [G5_QLBV] SET MULTI_USER;");
        }

        public (decimal TotalRevenue, int TotalPatients, int TotalRecords, int TotalAppointments) GetDashboardMetrics()
        {
            using var db = new HospitalDbContext();
            decimal revenue = db.Bills.Where(b => b.Status == "Đã thanh toán").Sum(b => b.TotalAmount ?? 0m);
            int patients = db.Patients.Count();
            int records = db.Medicalrecords.Count();
            int appts = db.Appointments.Count();
            return (revenue, patients, records, appts);
        }

        public List<DoctorPerformanceDto> GetDoctorPerformance()
        {
            using var db = new HospitalDbContext();
            return db.Medicalrecords
                .Include(r => r.Doctor)
                .Where(r => r.Doctor != null)
                .GroupBy(r => r.Doctor!.Fullname)
                .Select(g => new DoctorPerformanceDto
                {
                    DoctorName = g.Key ?? "N/A",
                    ExamCount = g.Count()
                })
                .OrderByDescending(x => x.ExamCount)
                .ToList();
        }

        public List<RevenueByDateDto> GetRevenueByDate()
        {
            using var db = new HospitalDbContext();
            return db.Bills
                .Where(b => b.Status == "Đã thanh toán" && b.CreatedAt != null)
                .AsEnumerable()
                .GroupBy(b => b.CreatedAt!.Value.ToString("dd/MM/yyyy"))
                .Select(g => new RevenueByDateDto
                {
                    DateLabel = g.Key,
                    Revenue = g.Sum(x => x.TotalAmount ?? 0m)
                })
                .TakeLast(10)
                .ToList();
        }

        private void LogCurrentUser(HospitalDbContext db, string action)
        {
            if (AuthService.CurrentSession != null && !AuthService.CurrentSession.IsPatient && AuthService.CurrentSession.Id > 0)
                _authService.WriteLog(db, AuthService.CurrentSession.Id, action);
        }
    }
}