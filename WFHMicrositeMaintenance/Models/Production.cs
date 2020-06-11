using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WFHMicrositeMaintenance.Models
{
    public class Production
    {
        public User User { get; set; }
        public Product Product { get; set; }
        public byte[] Image { get; set; }
        public List<UserSelection> UserSelections { get; set; }
        public List<ProductOption> ProductOptions { get; set; }
    }
}
