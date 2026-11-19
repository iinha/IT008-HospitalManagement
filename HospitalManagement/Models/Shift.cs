using System;
using System.Collections.Generic;

namespace HospitalManagement.Models;

public partial class Shift
{
    public int ShiftId { get; set; }

    public int? UserId { get; set; }

    public int? RoomId { get; set; }

    public DateTime? ShiftDate { get; set; }

    public string? TimeFrame { get; set; }

    public virtual Room? Room { get; set; }

    public virtual User? User { get; set; }
}
