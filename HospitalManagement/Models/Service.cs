using System;
using System.Collections.Generic;

namespace HospitalManagement.Models;

public partial class Service
{
    public int ServiceId { get; set; }

    public string? ServiceName { get; set; }

    public decimal? Price { get; set; }

    public string? Category { get; set; }

    public virtual ICollection<Servicedetail> Servicedetails { get; set; } = new List<Servicedetail>();
}
