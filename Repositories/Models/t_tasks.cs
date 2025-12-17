using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Repositories.CustomValidation;

namespace Repositories.Models
{
    public class t_tasks{
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int c_taskid { get; set; }

        public int? c_projectid{ get; set; }

        public int c_empid { get; set; }

        [Required(ErrorMessage ="Task title is required!")]
        [StringLength(150, ErrorMessage = "Title length must be less than 150!")]
        public string c_title { get; set; }


        [Required(ErrorMessage = "Description is required!")]
        [StringLength(500,ErrorMessage = "Description length must be less than 500!")]
        public string c_description { get; set; }

        [Required(ErrorMessage = "Estimated days is required!")]
        public int c_estimateddays { get; set; }

        [Required(ErrorMessage="Start Date is required!")]
        [DataType(DataType.Date, ErrorMessage = "Date formate is not valid!")]
        [FutureOrTodayDate]
        public DateTime c_startdate { get; set; }
         
        [DataType(DataType.Date, ErrorMessage = "Date formate is not valid!")]
        public DateTime? c_enddate { get; set; }
        
        [Required(ErrorMessage="Status is required!")]
        public string c_status{ get; set; }


        //employee panel change
          [NotMapped]
    public string c_projectname { get; set; } = string.Empty;
    }
}