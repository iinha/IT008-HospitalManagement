using System;
using System.Collections.Generic;

namespace HospitalManagement.Models;

public partial class User
{
    public int UserId { get; set; }

    public string? Username { get; set; }

    public string? PasswordHash { get; set; }

    public string? Fullname { get; set; }

    public string? Role { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<Article> Articles { get; set; } = new List<Article>();

    public virtual ICollection<Careorder> Careorders { get; set; } = new List<Careorder>();

    public virtual ICollection<Medicalrecord> MedicalrecordDoctors { get; set; } = new List<Medicalrecord>();

    public virtual ICollection<Medicalrecord> MedicalrecordNurses { get; set; } = new List<Medicalrecord>();

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual ICollection<Shift> Shifts { get; set; } = new List<Shift>();

    public virtual ICollection<Syslog> Syslogs { get; set; } = new List<Syslog>();
}
