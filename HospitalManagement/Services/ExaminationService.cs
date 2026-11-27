using System;
using System.Collections.Generic;
using System.Linq;
using HospitalManagement.Data;
using HospitalManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Services
{
    public class ExaminationService
    {
        public List<Appointment> GetTodayAppointments()
        {
            using var db = new HospitalDbContext();
            var today = DateOnly.FromDateTime(DateTime.Today);
            return db.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Where(a => a.ApptDate == today)
                .OrderBy(a => a.ApptId)
                .ToList();
        }

        public List<Appointment> GetAllPendingAppointments()
        {
            using var db = new HospitalDbContext();
            return db.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .OrderByDescending(a => a.ApptDate)
                .ThenBy(a => a.ApptId)
                .ToList();
        }

        public Medicalrecord CheckInAppointment(int apptId, int? doctorId, int? roomId, decimal examFee)
        {
            using var db = new HospitalDbContext();
            var appt = db.Appointments.First(a => a.ApptId == apptId);
            appt.Status = "Đợi khám";
            if (doctorId.HasValue) appt.DoctorId = doctorId.Value;

            if (roomId.HasValue)
            {
                var room = db.Rooms.FirstOrDefault(r => r.RoomId == roomId.Value);
                if (room != null) room.Status = "Đang sử dụng";
            }

            int nextRecordId = (db.Medicalrecords.Max(r => (int?)r.RecordId) ?? 0) + 1;
            var record = new Medicalrecord
            {
                RecordId = nextRecordId,
                PatientId = appt.PatientId,
                DoctorId = appt.DoctorId,
                VisitDate = DateTime.Now,
                IsInpatient = false,
                Status = "Chờ đo chỉ số"
            };
            db.Medicalrecords.Add(record);

            int nextBillId = (db.Bills.Max(b => (int?)b.BillId) ?? 0) + 1;
            db.Bills.Add(new Bill
            {
                BillId = nextBillId,
                RecordId = nextRecordId,
                TotalAmount = examFee,
                CreatedAt = DateTime.Now,
                BillType = "Phí khám",
                Status = "Chưa thanh toán"
            });

            db.SaveChanges();
            return record;
        }

        public List<Medicalrecord> GetQueueForDoctor(int doctorId)
        {
            using var db = new HospitalDbContext();
            return db.Medicalrecords
                .Include(r => r.Patient)
                .Include(r => r.Exammetric)
                .Include(r => r.Servicedetails).ThenInclude(s => s.Service)
                .Include(r => r.Prescriptiondetails).ThenInclude(p => p.Drug)
                .Where(r => r.DoctorId == doctorId && r.Status != "Đã khám")
                .OrderBy(r => r.VisitDate)
                .ToList();
        }

        public List<Medicalrecord> GetRecordsForNurse()
        {
            using var db = new HospitalDbContext();
            return db.Medicalrecords
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .Include(r => r.Exammetric)
                .Include(r => r.Servicedetails).ThenInclude(s => s.Service)
                .Where(r => r.Status != "Đã khám")
                .OrderByDescending(r => r.VisitDate)
                .ToList();
        }

        public void SaveExamMetrics(int recordId, int? nurseId, Exammetric metrics)
        {
            using var db = new HospitalDbContext();
            var record = db.Medicalrecords.FirstOrDefault(r => r.RecordId == recordId);
            if (record == null) return;

            if (nurseId.HasValue) record.NurseId = nurseId.Value;
            if (record.Status == "Chờ đo chỉ số") record.Status = "Đợi khám";

            var existing = db.Exammetrics.FirstOrDefault(e => e.RecordId == recordId);
            if (existing == null)
            {
                metrics.RecordId = recordId;
                db.Exammetrics.Add(metrics);
            }
            else
            {
                existing.VisionRight = metrics.VisionRight;
                existing.VisionLeft = metrics.VisionLeft;
                existing.IopRight = metrics.IopRight;
                existing.IopLeft = metrics.IopLeft;
                existing.SphRight = metrics.SphRight;
                existing.CylRight = metrics.CylRight;
                existing.AxisRight = metrics.AxisRight;
                existing.SphLeft = metrics.SphLeft;
                existing.CylLeft = metrics.CylLeft;
                existing.AxisLeft = metrics.AxisLeft;
            }
            db.SaveChanges();
        }

        public void SaveClinicalExamination(int recordId, string symptoms, string diagnosis, bool isInpatient, string status)
        {
            using var db = new HospitalDbContext();
            var record = db.Medicalrecords.FirstOrDefault(r => r.RecordId == recordId);
            if (record == null) return;

            record.Symptoms = symptoms;
            record.Diagnosis = diagnosis;
            record.IsInpatient = isInpatient;
            record.Status = status;

            if (status == "Đã khám")
            {
                var appt = db.Appointments
                    .Where(a => a.PatientId == record.PatientId && a.DoctorId == record.DoctorId && a.Status == "Đợi khám")
                    .OrderByDescending(a => a.ApptDate)
                    .FirstOrDefault();
                if (appt != null) appt.Status = "Đã khám";
            }

            db.SaveChanges();
        }

        public void OrderService(int recordId, int serviceId)
        {
            using var db = new HospitalDbContext();
            var service = db.Services.FirstOrDefault(s => s.ServiceId == serviceId);
            if (service == null) return;

            if (!db.Servicedetails.Any(sd => sd.RecordId == recordId && sd.ServiceId == serviceId))
            {
                db.Servicedetails.Add(new Servicedetail
                {
                    RecordId = recordId,
                    ServiceId = serviceId,
                    UnitPrice = service.Price ?? 0m,
                    PaymentStatus = "Chưa thanh toán",
                    ResultNote = string.Empty
                });

                int nextBillId = (db.Bills.Max(b => (int?)b.BillId) ?? 0) + 1;
                db.Bills.Add(new Bill
                {
                    BillId = nextBillId,
                    RecordId = recordId,
                    TotalAmount = service.Price ?? 0m,
                    CreatedAt = DateTime.Now,
                    BillType = $"Dịch vụ: {service.ServiceName}",
                    Status = "Chưa thanh toán"
                });

                var record = db.Medicalrecords.FirstOrDefault(r => r.RecordId == recordId);
                if (record != null) record.Status = "Chờ cận lâm sàng";

                db.SaveChanges();
            }
        }

        public void UpdateServiceResult(int recordId, int serviceId, string resultNote)
        {
            using var db = new HospitalDbContext();
            var detail = db.Servicedetails.FirstOrDefault(s => s.RecordId == recordId && s.ServiceId == serviceId);
            if (detail == null) return;
            detail.ResultNote = resultNote;
            db.SaveChanges();
        }

        public void AddPrescriptionItem(int recordId, int drugId, int quantity, string dosage)
        {
            using var db = new HospitalDbContext();
            var drug = db.Drugs.FirstOrDefault(d => d.DrugId == drugId);
            if (drug == null) return;

            var existing = db.Prescriptiondetails.FirstOrDefault(p => p.RecordId == recordId && p.DrugId == drugId);
            if (existing != null)
            {
                existing.Quantity = quantity;
                existing.Dosage = dosage;
                existing.UnitPrice = drug.UnitPrice ?? 0m;
            }
            else
            {
                db.Prescriptiondetails.Add(new Prescriptiondetail
                {
                    RecordId = recordId,
                    DrugId = drugId,
                    Quantity = quantity,
                    UnitPrice = drug.UnitPrice ?? 0m,
                    Dosage = dosage
                });
            }
            db.SaveChanges();
        }

        public void CreateDrugBillForRecord(int recordId)
        {
            using var db = new HospitalDbContext();
            var items = db.Prescriptiondetails.Where(p => p.RecordId == recordId).ToList();
            if (!items.Any()) return;

            decimal total = items.Sum(i => (i.Quantity ?? 0) * (i.UnitPrice ?? 0m));
            var existingBill = db.Bills.FirstOrDefault(b => b.RecordId == recordId && b.BillType == "Tiền thuốc");
            if (existingBill != null)
            {
                existingBill.TotalAmount = total;
            }
            else
            {
                int nextBillId = (db.Bills.Max(b => (int?)b.BillId) ?? 0) + 1;
                db.Bills.Add(new Bill
                {
                    BillId = nextBillId,
                    RecordId = recordId,
                    TotalAmount = total,
                    CreatedAt = DateTime.Now,
                    BillType = "Tiền thuốc",
                    Status = "Chưa thanh toán"
                });
            }
            db.SaveChanges();
        }

        public void CreateCareOrder(int recordId, int? nurseId, string actionName)
        {
            using var db = new HospitalDbContext();
            int nextId = (db.Careorders.Max(c => (int?)c.CareOrderId) ?? 0) + 1;
            db.Careorders.Add(new Careorder
            {
                CareOrderId = nextId,
                RecordId = recordId,
                NurseId = nurseId,
                ActionName = actionName,
                Status = "Chờ thực hiện",
                ExecuteAt = DateTime.Now
            });
            db.SaveChanges();
        }

        public List<Careorder> GetCareOrders()
        {
            using var db = new HospitalDbContext();
            return db.Careorders
                .Include(c => c.Record).ThenInclude(r => r!.Patient)
                .Include(c => c.Nurse)
                .OrderByDescending(c => c.CareOrderId)
                .ToList();
        }

        public void CompleteCareOrder(int careOrderId, int nurseId)
        {
            using var db = new HospitalDbContext();
            var order = db.Careorders.FirstOrDefault(c => c.CareOrderId == careOrderId);
            if (order == null) return;
            order.NurseId = nurseId;
            order.Status = "Hoàn thành";
            order.ExecuteAt = DateTime.Now;
            db.SaveChanges();
        }

        public List<Servicedetail> GetAllServiceOrders()
        {
            using var db = new HospitalDbContext();
            return db.Servicedetails
                .Include(s => s.Service)
                .Include(s => s.Record).ThenInclude(r => r!.Patient)
                .OrderByDescending(s => s.RecordId)
                .ToList();
        }

        public List<Room> GetRooms()
        {
            using var db = new HospitalDbContext();
            return db.Rooms.OrderBy(r => r.RoomId).ToList();
        }

        public void UpdateRoomStatus(int roomId, string status)
        {
            using var db = new HospitalDbContext();
            var room = db.Rooms.FirstOrDefault(r => r.RoomId == roomId);
            if (room == null) return;
            room.Status = status;
            db.SaveChanges();
        }

        public List<Shift> GetShifts(int? userId = null)
        {
            using var db = new HospitalDbContext();
            var query = db.Shifts.Include(s => s.User).Include(s => s.Room).AsQueryable();
            if (userId.HasValue) query = query.Where(s => s.UserId == userId.Value);
            return query.OrderByDescending(s => s.ShiftDate).ToList();
        }

        public void RegisterShift(int userId, int roomId, DateTime date, string timeFrame)
        {
            using var db = new HospitalDbContext();
            int nextId = (db.Shifts.Max(s => (int?)s.ShiftId) ?? 0) + 1;
            db.Shifts.Add(new Shift
            {
                ShiftId = nextId,
                UserId = userId,
                RoomId = roomId,
                ShiftDate = date,
                TimeFrame = timeFrame
            });
            db.SaveChanges();
        }

        public List<Question> GetDoctorQuestions(int doctorId)
        {
            using var db = new HospitalDbContext();
            return db.Questions
                .Include(q => q.Patient)
                .Where(q => q.DoctorId == doctorId || q.DoctorId == null)
                .OrderByDescending(q => q.CreatedAt)
                .ToList();
        }

        public void AnswerQuestion(int questionId, int doctorId, string answer)
        {
            using var db = new HospitalDbContext();
            var q = db.Questions.FirstOrDefault(x => x.QuestionId == questionId);
            if (q == null) return;
            q.DoctorId = doctorId;
            q.AnswerContent = answer;
            q.Status = "Đã trả lời";
            db.SaveChanges();
        }

        public List<Drug> GetDrugs()
        {
            using var db = new HospitalDbContext();
            return db.Drugs.OrderBy(d => d.DrugName).ToList();
        }

        public List<Service> GetServices()
        {
            using var db = new HospitalDbContext();
            return db.Services.OrderBy(s => s.ServiceName).ToList();
        }
    }
}