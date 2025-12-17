using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Models
{
    public class t_taskaccurarcy
    {
        [Required]
        public int TaskId { get; set; }

        public int EmpId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Project name is required")]
        public string ProjectName { get; set; }

        [Required(ErrorMessage = "Employee name is required")]
        public string EmployeeName { get; set; }

        [Required]
        public decimal? AccuracyPercentage { get; set; }
        [Required]
        public decimal? EfficiencyPercentage { get; set; }

        [Required]
        public int totaltasks { get; set; }

        [Required]
        public string Profileimage { get; set; }
    }
}