using System;
using System.Collections.Generic;

namespace WpfApp.Models;

public partial class PickupPoint
{
    public int Id { get; set; }

    public string PostalIndex { get; set; } = null!;

    public string City { get; set; } = null!;

    public string Street { get; set; } = null!;

    public string? BuildingNumber { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
