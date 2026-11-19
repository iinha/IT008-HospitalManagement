using System;
using System.Collections.Generic;

namespace HospitalManagement.Models;

public partial class Article
{
    public int ArticleId { get; set; }

    public string? Title { get; set; }

    public string? Content { get; set; }

    public string? Category { get; set; }

    public int? AuthorId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? Author { get; set; }
}
