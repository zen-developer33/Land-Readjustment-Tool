using Land_Readjustment_Tool.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Mathematics;
using System.Diagnostics.Metrics;

namespace Land_Readjustment_Tool.Data
{
    /// <summary>
    /// Used ONLY by EF Core migration commands at design time.
    /// Never used in actual application runtime.
    /// Provides a temporary dummy path so migrations can run.
    /// </summary>
    public class AppDbContextFactory
        : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Dummy path for design time only
            // Actual path is always provided at runtime
            // when user opens or creates a project
            return new AppDbContext("design_time_dummy.lpp");
        }
    }
}