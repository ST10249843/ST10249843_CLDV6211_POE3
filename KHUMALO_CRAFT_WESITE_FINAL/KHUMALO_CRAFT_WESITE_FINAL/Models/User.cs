using System;
using System.Collections.Generic;

namespace KHUMALO_CRAFT_WESITE_FINAL.Models;

public partial class User
{
    public int Userid { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
