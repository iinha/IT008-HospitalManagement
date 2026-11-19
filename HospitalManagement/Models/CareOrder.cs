using System;
using System.Collections.Generic;

namespace HospitalManagement.Models;

public partial class Careorder
{
    public int CareOrderId { get; set; }

    public int? RecordId { get; set; }

    public int? NurseId { get; set; }

    public string? ActionName { get; set; }

    public string? Status { get; set; }

    public DateTime? ExecuteAt { get; set; }

    public virtual User? Nurse { get; set; }

    public virtual Medicalrecord? Record { get; set; }
}
