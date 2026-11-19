using System;
using System.Collections.Generic;

namespace HospitalManagement.Models;

public partial class Prescriptiondetail
{
    public int RecordId { get; set; }

    public int DrugId { get; set; }

    public int? Quantity { get; set; }

    public decimal? UnitPrice { get; set; }

    public string? Dosage { get; set; }

    public virtual Drug Drug { get; set; } = null!;

    public virtual Medicalrecord Record { get; set; } = null!;
}
