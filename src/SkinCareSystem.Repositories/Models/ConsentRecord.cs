using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class ConsentRecord
{
    public Guid ConsentId { get; set; }

    public Guid UserId { get; set; }

    public string ConsentType { get; set; } = null!;

    public string ConsentText { get; set; } = null!;

    public bool Given { get; set; }

    public DateTime? GivenAt { get; set; }

    public virtual User User { get; set; } = null!;
}
