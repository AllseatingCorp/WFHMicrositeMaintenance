using System;
using System.Collections.Generic;

namespace WFHMicrositeMaintenance.Models
{
    public partial class User
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public string EmailAddress { get; set; }
        public string Language { get; set; }
        public string Pin { get; set; }
        public string AttnName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string ProvinceState { get; set; }
        public string PostalZip { get; set; }
        public string Country { get; set; }
        public string SpecialInstructions { get; set; }
        public bool Commercial { get; set; }
        public DateTime? Emailed { get; set; }
        public DateTime? Submitted { get; set; }
        public DateTime? Completed { get; set; }
        public DateTime? InProduction { get; set; }
        public DateTime? Shipped { get; set; }
        public string TrackingNumber { get; set; }
        public string SessionId { get; set; }
    }
}
