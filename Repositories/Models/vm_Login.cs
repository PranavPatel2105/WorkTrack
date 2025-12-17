using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
namespace Repositories.Models
{
    public class vm_Login
    {
        [StringLength(100)]
        [RegularExpression(@"^[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-zA-Z]{2,}$",ErrorMessage = "Invalid email format.")]
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string c_email { get; set; }
       
        [StringLength(100)]
        [Required(ErrorMessage = "Password is required.")]
        public string c_password { get; set; }
    }
}