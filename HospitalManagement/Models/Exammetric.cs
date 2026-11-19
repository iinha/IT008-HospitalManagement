using System;
using System.Collections.Generic;

namespace HospitalManagement.Models;

public partial class Exammetric
{
    public int RecordId { get; set; }

    public double? VisionRight { get; set; }

    public double? VisionLeft { get; set; }

    public double? IopRight { get; set; }

    public double? IopLeft { get; set; }

    public double? SphRight { get; set; }

    public double? CylRight { get; set; }

    public double? AxisRight { get; set; }

    public double? SphLeft { get; set; }

    public double? CylLeft { get; set; }

    public double? AxisLeft { get; set; }

    public virtual Medicalrecord Record { get; set; } = null!;
}
