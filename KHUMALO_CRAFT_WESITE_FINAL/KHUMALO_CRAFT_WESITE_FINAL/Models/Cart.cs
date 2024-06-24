using System;
using System.Collections.Generic;

namespace KHUMALO_CRAFT_WESITE_FINAL.Models;

public partial class Cart
{
    public int CartId { get; set; }

    public int? ProductId { get; set; }

    public int? Userid { get; set; }

    public virtual Product? Product { get; set; }

    public virtual User? User { get; set; }
}
