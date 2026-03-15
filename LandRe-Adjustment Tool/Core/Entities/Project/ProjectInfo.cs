using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Land_Readjustment_Tool.Core.Entities.Project
{
    [Table("tblProjectInfo")]
    public class ProjectInfo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ProjectName { get; set; } = string.Empty;

        public string? Province { get; set; } 
        public string? District { get; set; } 
        public string? Municipality { get; set; }
        public string? WardNo { get; set; } 
        public string? ProjectSite { get; set; } 
        public string? ImplementingAgency { get; set; } 
        public string? ConsultingAgency { get;set;  } 
        public string? GazetteNotificationNumber { get; set;  }  
        public DateTime? GazzeteDate { get; set; } 
        public DateTime? ProjectStartDate {  get; set; }
        public DateTime? ProjectEndDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string? ProjectNotes { get; set; } = string.Empty ;

    }
}

