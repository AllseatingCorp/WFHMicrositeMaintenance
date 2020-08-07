using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WFHMicrositeMaintenance.Models
{
    public partial class SearchData
    {
        public int Fabric { get; set; }
        public int Mesh { get; set; }
        public int Frame { get; set; }
        public string Tracking { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Completed { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime InProduction { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Shipped { get; set; }
    }
}