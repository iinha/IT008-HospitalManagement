using System;
using System.Collections.Generic;

namespace HospitalManagement.Models;

public partial class Servicedetail
{
    public int RecordId { get; set; }

    public int ServiceId { get; set; }

    public string? ResultNote { get; set; }

    public decimal? UnitPrice { get; set; }

    public string? PaymentStatus { get; set; }

    public virtual Medicalrecord Record { get; set; } = null!;

    public virtual Service Service { get; set; } = null!;
}
