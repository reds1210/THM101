using System;
using System.Collections.Generic;

namespace WebApplication4.Models;

public partial class User
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Account { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Role { get; set; } = null!;
    public bool IsActive { get; set; }

    public virtual ICollection<Income> Incomes { get; set; } = new List<Income>();
}
