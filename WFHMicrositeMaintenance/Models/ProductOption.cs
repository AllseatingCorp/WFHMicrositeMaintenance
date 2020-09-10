using System;
using System.Collections.Generic;

namespace WFHMicrositeMaintenance.Models
{
    public partial class ProductOption
    {
        public int ProductOptionId { get; set; }
        public int ProductId { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        public byte[] Image { get; set; }
        public bool Default { get; set; }
        public string StockCode { get; set; }
        public bool? Disabled { get; set; }
    }
}
