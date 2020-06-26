using System;
using System.Collections.Generic;

namespace WFHMicrositeMaintenance.Models
{
    public partial class Selections
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public string EmailAddress { get; set; }
        public int Fabric { get; set; }
        public int Mesh { get; set; }
        public int Frame { get; set; }
        public List<UserSelection> UserSelections { get; set; }
    }
}
