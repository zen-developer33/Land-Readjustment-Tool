using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using Land_Readjustment_Tool.Models;

namespace Land_Readjustment_Tool.Repositories
{
    internal class ProjectInfoRepository
    {
        private readonly SQLiteConnection _connection;

        public ProjectInfoRepository(SQLiteConnection connection)
        {
            _connection = connection;
        }
        public void SaveProjectInfo(ProjectInfo Info)
        {
            string sql = @"
            INSERT OR REPLACE INTO ProjectInfo (
                GUID, ProjectName, ProjectPath, CreatedDate, ApprovalDate,
                Province, District, Municipality, WardNo, ProjectSite,
                ImplementingAgency, ConsultingAgency
            )
            VALUES (
                @GUID, @ProjectName, @ProjectPath, @CreatedDate, @ApprovalDate,
                @Province, @District, @Municipality, @WardNo, @ProjectSite,
                @ImplementingAgency, @ConsultingAgency
            );";

            using var cmd = new SQLiteCommand(sql, _connection);
            cmd.Parameters.AddWithValue("@GUID", Info.GUID.ToString());
            cmd.Parameters.AddWithValue("@ProjectName", Info.ProjectName);
            cmd.Parameters.AddWithValue("@ProjectPath", Info.ProjectPath);
            cmd.Parameters.AddWithValue("@CreatedDate", Info.CreatedDate.ToString("s"));
            cmd.Parameters.AddWithValue("@ApprovalDate", Info.ApprovalDate.ToString("s"));

            cmd.Parameters.AddWithValue("@Province", Info.Location.Province);
            cmd.Parameters.AddWithValue("@District", Info.Location.District);
            cmd.Parameters.AddWithValue("@Municipality", Info.Location.Municipality);
            cmd.Parameters.AddWithValue("@WardNo", Info.Location.WardNo);
            cmd.Parameters.AddWithValue("@ProjectSite", Info.Location.ProjectSite);

            cmd.Parameters.AddWithValue("@ImplementingAgency", Info.Stakeholders.ImplementingAgency);
            cmd.Parameters.AddWithValue("@ConsultingAgency", Info.Stakeholders.ConsultingAgency);

            if (cmd.ExecuteNonQuery() > 0)
            {
                CurrentProject.MarkAsModified();
            }

        }

        public ProjectInfo? GetProjectInfo()
        {
            string sql = "SELECT * FROM ProjectInfo ;";
            using var cmd = new SQLiteCommand(@sql, _connection);
            using var reader = cmd.ExecuteReader();
            if(!reader.Read()) return null;

            ProjectInfo info = new ProjectInfo()
            {
                GUID = Guid.Parse(reader["GUID"].ToString()!),
                ProjectName = reader["ProjectName"].ToString()!,
                ProjectPath = reader["ProjectPath"].ToString()!,
                CreatedDate = DateTime.Parse(reader["CreatedDate"].ToString()!),
                ApprovalDate = DateTime.Parse(reader["ApprovalDate"].ToString()!),

                Location = new ProjectLocation
                {
                    Province = reader["Province"].ToString()!,
                    District = reader["District"].ToString()!,
                    Municipality = reader["Municipality"].ToString()!,
                    WardNo = reader["WardNo"].ToString()!,
                    ProjectSite = reader["ProjectSite"].ToString()!
                },

                Stakeholders = new ProjectStakeholders
                {
                    ImplementingAgency = reader["ImplementingAgency"].ToString()!,
                    ConsultingAgency = reader["ConsultingAgency"].ToString()!
                }
            };

            return info;
        }



    }
}
