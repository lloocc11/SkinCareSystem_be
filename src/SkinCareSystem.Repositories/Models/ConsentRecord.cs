using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class ConsentRecord
{
    public Guid consent_id { get; set; }

    public Guid user_id { get; set; }

    public string consent_type { get; set; } = null!;

    public string consent_text { get; set; } = null!;

    public bool given { get; set; }

    public DateTime? given_at { get; set; }

    public virtual User user { get; set; } = null!;
}
