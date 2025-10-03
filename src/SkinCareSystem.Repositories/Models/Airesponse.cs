using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class Airesponse
{
    public Guid ResponseId { get; set; }

    public Guid QueryId { get; set; }

    public string ResponseText { get; set; } = null!;

    public string ResponseType { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual UserQuery Query { get; set; } = null!;
}
