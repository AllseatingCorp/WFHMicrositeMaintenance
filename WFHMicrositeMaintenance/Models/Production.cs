using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WFHMicrositeMaintenance.Models
{
    public class Production
    {
        public User User { get; set; }
        public Product Product { get; set; }
        public byte[] Image { get; set; }
        public List<UserSelection> UserSelections { get; set; }
        public List<ProductOption> ProductOptions { get; set; }
        public List<ProductionList> List { get; set; }
        public SelectList Fabrics { get; set; }
        public SelectList Meshs { get; set; }
        public SelectList Frames { get; set; }
    }

    public class ProductionList
    {
        public User User { get; set; }
        public Product Product { get; set; }
        public byte[] Image { get; set; }
        public List<UserSelection> UserSelections { get; set; }
        public List<ProductOption> ProductOptions { get; set; }
    }
}
