using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class UserQuery
{
    public Guid QueryId { get; set; }

    public Guid UserId { get; set; }

    public string QueryText { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Airesponse> Airesponses { get; set; } = new List<Airesponse>();

    public virtual ICollection<QueryMatch> QueryMatches { get; set; } = new List<QueryMatch>();

    public virtual User User { get; set; } = null!;
}
