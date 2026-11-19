using System;
using System.Collections.Generic;

namespace HospitalManagement.Models;

public partial class Medicalrecord
{
    public int RecordId { get; set; }

    public int? PatientId { get; set; }

    public int? NurseId { get; set; }

    public int? DoctorId { get; set; }

    public DateTime? VisitDate { get; set; }

    public string? Symptoms { get; set; }

    public string? Diagnosis { get; set; }

    public bool? IsInpatient { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();

    public virtual ICollection<Careorder> Careorders { get; set; } = new List<Careorder>();

    public virtual User? Doctor { get; set; }

    public virtual Exammetric? Exammetric { get; set; }

    public virtual User? Nurse { get; set; }

    public virtual Patient? Patient { get; set; }

    public virtual ICollection<Prescriptiondetail> Prescriptiondetails { get; set; } = new List<Prescriptiondetail>();

    public virtual ICollection<Servicedetail> Servicedetails { get; set; } = new List<Servicedetail>();
}
