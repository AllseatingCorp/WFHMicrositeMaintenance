using System;
using System.Collections.Generic;

namespace WFHMicrositeMaintenance.Models
{
    public partial class Product
    {
        public int ProductId { get; set; }
        public string DealerCode { get; set; }
        public string Name { get; set; }
        public string Ponumber { get; set; }
        public string Chair { get; set; }
        public string Language { get; set; }
        public string LogoFile { get; set; }
        public byte[] LogoImage { get; set; }
        public string LogoFile2 { get; set; }
        public byte[] LogoImage2 { get; set; }
        public string InstallGuide { get; set; }
        public string UserGuide { get; set; }
        public string VideoUrl { get; set; }
        public string SitFitGuide { get; set; }
        public bool VerifyOnly { get; set; }
        public bool Completed { get; set; }
    }
}
