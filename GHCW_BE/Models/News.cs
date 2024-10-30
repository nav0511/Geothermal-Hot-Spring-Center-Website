using System;
using System.Collections.Generic;

namespace GHCW_BE.Models
{
    public partial class News
    {
        public int Id { get; set; }
        public string? DiscountId { get; set; }
        public string Title { get; set; } = null!;
        public DateTime UploadDate { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public bool IsActive { get; set; }

        public virtual Discount? Discount { get; set; }
    }
}
