using System;
using System.Collections.Generic;

namespace SkinCareSystem.Repositories.Models;

public partial class Role
{
    public Guid role_id { get; set; }

    public string name { get; set; } = null!;

    public string? description { get; set; }

    public string status { get; set; } = null!;

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
