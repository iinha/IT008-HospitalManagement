using System;
using System.Collections.Generic;

namespace HospitalManagement.Models;

public partial class Question
{
    public int QuestionId { get; set; }

    public int? PatientId { get; set; }

    public int? DoctorId { get; set; }

    public string? Content { get; set; }

    public string? AnswerContent { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? Doctor { get; set; }

    public virtual Patient? Patient { get; set; }
}
