using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WFHMicrositeMaintenance.Models
{
    public class Login
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
