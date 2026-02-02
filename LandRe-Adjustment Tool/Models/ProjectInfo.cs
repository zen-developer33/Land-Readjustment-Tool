using CsvHelper;
using System;

namespace Land_Readjustment_Tool.Models
{
    public class ProjectInfo
    {
        // Category: General Meta Data
        public Guid GUID { get; set; } = Guid.NewGuid(); // Unique identifier for DB
        public string ProjectName { get; set; } = "";
        public string ProjectPath { get; set; } = "";
        public DateTime CreatedDate { get; set; }
        public DateTime ApprovalDate { get; set; }

        // Category: Location Details
        public ProjectLocation Location { get; set; } = new();

        // Category: Stakeholders
        public ProjectStakeholders Stakeholders { get; set; } = new();
    }

    // Add this new class for global access
   

    public class ProjectLocation
    {
        public string Province { get; set; } = "";
        public string District { get; set; } = "";
        public string Municipality { get; set; } = "";
        public string WardNo { get; set; } = "";
        public string ProjectSite { get; set; } = "";
    }

    public class ProjectStakeholders
    {
        public string ImplementingAgency { get; set; } = ""; // Fixed typo: Implementing
        public string ConsultingAgency { get; set; } = "";
    }

    public static class CurrentProject
    {
        public static event Action? StateChanged;
        public static ProjectInfo? Info { get; set; }
        public static bool HasUnsavedChanges { get; set; }
        public static bool IsOpen => Info != null;
        public static void MarkAsModified()
        {
            HasUnsavedChanges = true;
            StateChanged?.Invoke();
        }

        public static void MarkAsSaved()
        {
            HasUnsavedChanges = false;
            StateChanged?.Invoke();
        }

        public static void Close()
        {
            Info = null;
            HasUnsavedChanges = false;
            StateChanged?.Invoke();
        }
    }



}


