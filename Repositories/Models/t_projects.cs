using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Models
{
    public class t_projects
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int c_projectid { get; set; }

        [Required(ErrorMessage = "Project is required")]
        [MinLength(5, ErrorMessage = "Project must be at least 5 characters")]
        [RegularExpression("^[A-Za-z0-9 ]+$", ErrorMessage = "Project name can contain only letters , numbers and spaces")]

        public string c_projectname { get; set; }

        [Required(ErrorMessage = "Project Description is required")]
        [MinLength(20, ErrorMessage = "Project must be at least 20 characters")]    
        public string c_description { get; set; }
    }
}