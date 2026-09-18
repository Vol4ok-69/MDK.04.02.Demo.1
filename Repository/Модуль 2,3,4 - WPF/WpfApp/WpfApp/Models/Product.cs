using System;
using System.Collections.Generic;

namespace WpfApp.Models;

public partial class Product
{
    public int Id { get; set; }

    public string SKU { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int UnitOfMeasurementId { get; set; }

    public decimal Price { get; set; }

    public int SupplierId { get; set; }

    public int ManufacturerId { get; set; }

    public int ProductCategoryId { get; set; }

    public decimal CurrentDiscount { get; set; }

    public int StockQuantity { get; set; }

    public string Description { get; set; } = null!;

    public string? Photo { get; set; }

    public virtual Manufacturer Manufacturer { get; set; } = null!;

    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();

    public virtual Category ProductCategory { get; set; } = null!;

    public virtual Supplier Supplier { get; set; } = null!;

    public virtual Unit UnitOfMeasurement { get; set; } = null!;
}
