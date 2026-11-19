using System;
using System.Collections.Generic;

namespace HospitalManagement.Models;

public partial class Drug
{
    public int DrugId { get; set; }

    public string? DrugName { get; set; }

    public string? Unit { get; set; }

    public decimal? UnitPrice { get; set; }

    public int? StockQuantity { get; set; }

    public virtual ICollection<Prescriptiondetail> Prescriptiondetails { get; set; } = new List<Prescriptiondetail>();
}
