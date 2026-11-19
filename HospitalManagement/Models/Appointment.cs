using System;
using System.Collections.Generic;

namespace HospitalManagement.Models;

public partial class Appointment
{
    public int ApptId { get; set; }

    public int? PatientId { get; set; }

    public int? DoctorId { get; set; }

    public DateOnly? ApptDate { get; set; }

    public string? TimeSlot { get; set; }

    public string? Status { get; set; }

    public virtual User? Doctor { get; set; }

    public virtual Patient? Patient { get; set; }
}
