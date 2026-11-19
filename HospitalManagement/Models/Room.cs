using System;
using System.Collections.Generic;

namespace HospitalManagement.Models;

public partial class Room
{
    public int RoomId { get; set; }

    public string? RoomName { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<Shift> Shifts { get; set; } = new List<Shift>();
}
