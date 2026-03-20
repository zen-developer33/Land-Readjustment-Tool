using Land_Readjustment_Tool.Core.Entities.Project;
using System;
using System.Collections.Generic;
using System.Text;

namespace Land_Readjustment_Tool.Core.Interfaces
{

    /// <summary>
    /// Contract for Project Seting Service.
    /// Forms depnd on tthis interface, not the concrete class.
    /// </summary>
    public interface IProjectSettingsService
    {
        ///<summary>
        ///Gets project settings from the Database.
        ///Returns null if not found.
        ///</summary>
        Task<ProjectSettings?> GetAsync(CancellationToken ct = default) ;

        ///<summary>
        ///Validates and saves project Settings.
        ///Throws InvalidOperationException for rule violations.
        /// </summary>
        Task SaveAsync(ProjectSettings settings, CancellationToken ct = default);

        ///<summary>
        ///Marks Settings as configured.
        ///Called after user confirms settings window.
        ///IsConfigured = true prevents auto-opening.
        
        Task MarkAsConfiguredAsync(CancellationToken ct = default);

    }
}
