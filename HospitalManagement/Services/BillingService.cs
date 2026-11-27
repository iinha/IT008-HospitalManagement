using System;
using System.Collections.Generic;
using System.Linq;
using HospitalManagement.Data;
using HospitalManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Services
{
    public class BillingService
    {
        public List<Bill> GetAllBills()
        {
            using var db = new HospitalDbContext();
            return db.Bills
                .Include(b => b.Record).ThenInclude(r => r!.Patient)
                .OrderByDescending(b => b.CreatedAt)
                .ToList();
        }

        public void PayBill(int billId)
        {
            using var db = new HospitalDbContext();
            var bill = db.Bills.FirstOrDefault(b => b.BillId == billId);
            if (bill == null) return;

            bill.Status = "Đã thanh toán";

            if (bill.RecordId.HasValue && bill.BillType != null && bill.BillType.StartsWith("Dịch vụ:"))
            {
                var unpaidServices = db.Servicedetails
                    .Where(s => s.RecordId == bill.RecordId.Value && s.PaymentStatus != "Đã thanh toán")
                    .ToList();
                foreach (var s in unpaidServices)
                {
                    s.PaymentStatus = "Đã thanh toán";
                }
            }

            db.SaveChanges();
        }

        public List<Medicalrecord> GetPendingPrescriptions()
        {
            using var db = new HospitalDbContext();
            return db.Medicalrecords
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .Include(r => r.Prescriptiondetails).ThenInclude(p => p.Drug)
                .Include(r => r.Bills)
                .Where(r => r.Prescriptiondetails.Any())
                .OrderByDescending(r => r.VisitDate)
                .ToList();
        }

        public bool DispensePrescription(int recordId, out string message)
        {
            message = string.Empty;
            using var db = new HospitalDbContext();
            var items = db.Prescriptiondetails
                .Include(p => p.Drug)
                .Where(p => p.RecordId == recordId)
                .ToList();

            if (!items.Any())
            {
                message = "Hồ sơ không có đơn thuốc.";
                return false;
            }

            foreach (var item in items)
            {
                int qty = item.Quantity ?? 0;
                int stock = item.Drug?.StockQuantity ?? 0;
                if (stock < qty)
                {
                    message = $"Thuốc '{item.Drug?.DrugName}' không đủ tồn kho (Còn {stock}, cần {qty}).";
                    return false;
                }
            }

            foreach (var item in items)
            {
                if (item.Drug != null)
                    item.Drug.StockQuantity = (item.Drug.StockQuantity ?? 0) - (item.Quantity ?? 0);
            }

            var drugBill = db.Bills.FirstOrDefault(b => b.RecordId == recordId && b.BillType == "Tiền thuốc");
            if (drugBill != null) drugBill.Status = "Đã thanh toán";

            var record = db.Medicalrecords.FirstOrDefault(r => r.RecordId == recordId);
            if (record != null) record.Status = "Đã phát thuốc";

            db.SaveChanges();
            message = "Đã phát thuốc và trừ tồn kho thành công.";
            return true;
        }
    }
}