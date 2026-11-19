using System;
using System.Collections.Generic;

namespace HospitalManagement.Models;

public partial class Patient
{
    public int PatientId { get; set; }

    public string? Fullname { get; set; }

    public string? PasswordHash { get; set; }

    public string? Phone { get; set; }

    public DateTime? DateofBirth { get; set; }

    public string? Gender { get; set; }

    public string? HealthInsuranceCode { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<Medicalrecord> Medicalrecords { get; set; } = new List<Medicalrecord>();

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
