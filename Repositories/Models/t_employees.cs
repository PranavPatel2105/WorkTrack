using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace Repositories.Models
{
    public class t_employees
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int c_empid { get; set; }

        [Required(ErrorMessage = "Employee name is required")]
        [MinLength(2, ErrorMessage = "Employee name must be at least 2 characters")]
        [RegularExpression("^[A-Za-z ]+$", ErrorMessage = "Employee name can contain only alphabets")]
        public string c_ename { get; set; }

        [Required(ErrorMessage = "Email id is required")]
        [RegularExpression(@"^[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string c_email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MaxLength(12, ErrorMessage = "Password can't exceed 12 characters")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[^A-Za-z\d]).{6,12}$", ErrorMessage = "Password must contain at least one letter, one number, one special character and at least 6 characters")]
        public string c_password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare("c_password", ErrorMessage = "Passwords do not match")]
        public string c_confirmpassword { get; set; }

        //new code
        [StringLength(20)]
        [Required]
        public string c_role { get; set; } = "Employee";


        [StringLength(255)]
        public string? c_profileimage { get; set; }

        [Required(ErrorMessage = "Profile Image is required.")]
        public IFormFile profilepicture { get; set; }
    }
}