using System;
using System.Collections.Generic;

namespace HospitalManagement.Models;

public partial class Syslog
{
    public int LogId { get; set; }

    public int? UserId { get; set; }

    public string? Action { get; set; }

    public DateTime? LoggedAt { get; set; }

    public virtual User? User { get; set; }
}
