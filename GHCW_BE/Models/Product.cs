using System;
using System.Collections.Generic;

namespace GHCW_BE.Models
{
    public partial class Product
    {
        public Product()
        {
            BillDetails = new HashSet<BillDetail>();
        }

        public int Id { get; set; }
        public int? CategoryId { get; set; }
        public string? Name { get; set; }
        public string? Size { get; set; }
        public string? Description { get; set; }
        public double? Price { get; set; }
        public bool? IsForRent { get; set; }
        public string? Image { get; set; }
        public int? Quantity { get; set; }
        public bool? IsAvailable { get; set; }

        public virtual Category? Category { get; set; }
        public virtual ICollection<BillDetail> BillDetails { get; set; }
    }
}
