using System;
using System.Collections.Generic;

namespace WFHMicrositeMaintenance.Models
{
    public partial class AlternatePonumbers
    {
        public int UserId { get; set; }
        public string AlternatePonumber { get; set; }
        public string PgmId { get; set; }
        public string Wo { get; set; }
    }
}
