using System;
using System.Collections.Generic;

namespace HospitalManagement.Models;

public partial class Bill
{
    public int BillId { get; set; }

    public int? RecordId { get; set; }

    public decimal? TotalAmount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? BillType { get; set; }

    public string? Status { get; set; }

    public virtual Medicalrecord? Record { get; set; }
}
