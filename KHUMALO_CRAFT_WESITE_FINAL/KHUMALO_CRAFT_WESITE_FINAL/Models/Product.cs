using System;
using System.Collections.Generic;

namespace KHUMALO_CRAFT_WESITE_FINAL.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public string ProductDescription { get; set; } = null!;

    public string ProductCategory { get; set; } = null!;

    public string ProductAvailability { get; set; } = null!;

    public double ProductPrice { get; set; }

    public string ProductImage { get; set; } = null!;

    public int? Userid { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual User? User { get; set; }
}
