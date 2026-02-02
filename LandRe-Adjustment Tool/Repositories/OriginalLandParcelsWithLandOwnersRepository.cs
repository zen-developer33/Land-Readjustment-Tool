
//using Land_Readjustment_Tool.Models;
//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Data.SQLite;

//namespace Land_Readjustment_Tool.Repositories
//{
//    /// <summary>
//    /// Repository for managing raw/dirty land owner records from Excel import.
//    /// Saves data AS-IS without validation - for import history and later processing.
//    /// </summary>
//    public class OriginalLandParcelsWithLandOwnersRepository
//    {
//        //private readonly DatabaseHelper _db;

//        public OriginalLandParcelsWithLandOwnersRepository(DatabaseHelper db)
//        {
//            _db = db;
//        }

//        /// <summary>
//        /// Creates the OriginalLandParcelWithLandOwner table if it doesn't exist.
//        /// This is a flat table storing ALL imported data as-is.
//        /// </summary>
//        public void CreateTableIfNotExists()
//        {
//            string createTableSql = @"
//                CREATE TABLE IF NOT EXISTS OriginalLandParcelWithLandOwner (
//                    RecordId INTEGER PRIMARY KEY AUTOINCREMENT,
//                    ParcelNo TEXT,
//                    Province TEXT,
//                    District TEXT,
//                    MunicipalityVillage TEXT,
//                    MapSheetNo TEXT,
//                    LandOwnersName TEXT,
//                    FatherSpouse TEXT,
//                    Gender TEXT,
//                    CitizenshipNumber TEXT,
//                    IsTenant TEXT,
//                    Address TEXT,
//                    LandUse TEXT,
//                    AreaInSqm REAL,
//                    AreaInRAPD TEXT,
//                    AreaInBKD TEXT,
//                    MothNo TEXT,
//                    PaanaNo TEXT,
//                    //Remarks TEXT,
//                    ImportedOn TEXT DEFAULT CURRENT_TIMESTAMP,
//                    IsValid INTEGER DEFAULT 1
//                );
//            ";

//            _db.ExecuteNonQuery(createTableSql);
//        }

//        /// <summary>
//        /// Bulk insert multiple records in a single transaction.
//        /// Returns the number of successfully inserted records.
//        /// </summary>
//        public int BulkInsert(List<OriginalLandParcelWithLandOwner> records)
//        {
//            if (records == null || records.Count == 0)
//                return 0;

//            int insertedCount = 0;

//            _db.ExecuteInTransaction(conn =>
//            {
//                string insertSql = @"
//                    INSERT INTO OriginalLandParcelWithLandOwner (
//                        ParcelNo, Province, District, MunicipalityVillage, MapSheetNo,
//                        LandOwnersName, FatherSpouse, Gender, CitizenshipNumber, IsTenant,
//                        Address, LandUse, AreaInSqm, AreaInRAPD, AreaInBKD,
//                        MothNo, PaanaNo, Remarks, IsValid
//                    ) VALUES (
//                        @ParcelNo, @Province, @District, @MunicipalityVillage, @MapSheetNo,
//                        @LandOwnersName, @FatherSpouse, @Gender, @CitizenshipNumber, @IsTenant,
//                        @Address, @LandUse, @AreaInSqm, @AreaInRAPD, @AreaInBKD,
//                        @MothNo, @PaanaNo, @Remarks, @IsValid
//                    )";

//                foreach (var record in records)
//                {
//                    using (var cmd = new SQLiteCommand(insertSql, conn))
//                    {
//                        cmd.Parameters.AddWithValue("@ParcelNo", record.ParcelNo ?? "");
//                        cmd.Parameters.AddWithValue("@Province", record.Province ?? "");
//                        cmd.Parameters.AddWithValue("@District", record.District ?? "");
//                        cmd.Parameters.AddWithValue("@MunicipalityVillage", record.MunicipalityVillage ?? "");
//                        cmd.Parameters.AddWithValue("@MapSheetNo", record.MapSheetNo ?? "");
//                        cmd.Parameters.AddWithValue("@LandOwnersName", record.LandOwnersName ?? "");
//                        cmd.Parameters.AddWithValue("@FatherSpouse", record.FatherSpouse ?? "");
//                        cmd.Parameters.AddWithValue("@Gender", record.Gender ?? "");
//                        cmd.Parameters.AddWithValue("@CitizenshipNumber", record.CitizenshipNumber ?? "");
//                        cmd.Parameters.AddWithValue("@IsTenant", record.IsTenant ?? "");
//                        cmd.Parameters.AddWithValue("@Address", record.Address ?? "");
//                        cmd.Parameters.AddWithValue("@LandUse", record.LandUse ?? "");
//                        cmd.Parameters.AddWithValue("@AreaInSqm", record.AreaInSqm ?? (object)DBNull.Value);
//                        cmd.Parameters.AddWithValue("@AreaInRAPD", record.AreaInRAPD ?? "");
//                        cmd.Parameters.AddWithValue("@AreaInBKD", record.AreaInBKD ?? "");
//                        cmd.Parameters.AddWithValue("@MothNo", record.MothNo ?? "");
//                        cmd.Parameters.AddWithValue("@PaanaNo", record.PaanaNo ?? "");
//                        cmd.Parameters.AddWithValue("@Remarks", record.Remarks ?? "");
//                        cmd.Parameters.AddWithValue("@IsValid", 1); // Default to valid

//                        cmd.ExecuteNonQuery();
//                        insertedCount++;
//                    }
//                }
//            });

//            return insertedCount;
//        }

//        /// <summary>
//        /// Insert a single record.
//        /// </summary>
//        public void Insert(OriginalLandParcelWithLandOwner record)
//        {
//            BulkInsert(new List<OriginalLandParcelWithLandOwner> { record });
//        }

//        /// <summary>
//        /// Get all records from database.
//        /// </summary>
//        public List<OriginalLandParcelWithLandOwner> GetAll()
//        {
//            var records = new List<OriginalLandParcelWithLandOwner>();

//            string sql = "SELECT * FROM OriginalLandParcelWithLandOwner ORDER BY RecordId";
//            DataTable dt = _db.ExecuteQuery(sql);

//            foreach (DataRow row in dt.Rows)
//            {
//                records.Add(MapRowToRecord(row));
//            }

//            return records;
//        }

//        /// <summary>
//        /// Get records by MapSheet.
//        /// </summary>
//        public List<OriginalLandParcelWithLandOwner> GetByMapSheet(string mapSheet)
//        {
//            var records = new List<OriginalLandParcelWithLandOwner>();

//            string sql = "SELECT * FROM OriginalLandParcelWithLandOwner WHERE MapSheetNo = @MapSheet";
//            DataTable dt = _db.ExecuteQuery(sql,
//                new SQLiteParameter("@MapSheet", mapSheet));

//            foreach (DataRow row in dt.Rows)
//            {
//                records.Add(MapRowToRecord(row));
//            }

//            return records;
//        }

//        /// <summary>
//        /// Check if a parcel already exists.
//        /// </summary>
//        public bool ParcelExists(string parcelNo, string mapSheet)
//        {
//            string sql = @"
//                SELECT COUNT(*) FROM OriginalLandParcelWithLandOwner 
//                WHERE ParcelNo = @ParcelNo AND MapSheetNo = @MapSheet";

//            object result = _db.ExecuteScalar(sql,
//                new SQLiteParameter("@ParcelNo", parcelNo),
//                new SQLiteParameter("@MapSheet", mapSheet));

//            return Convert.ToInt32(result) > 0;
//        }

//        /// <summary>
//        /// Delete all records (use with caution!).
//        /// </summary>
//        public void DeleteAll()
//        {
//            _db.ExecuteNonQuery("DELETE FROM OriginalLandParcelWithLandOwner");
//        }

//        /// <summary>
//        /// Get total record count.
//        /// </summary>
//        public int GetTotalCount()
//        {
//            object result = _db.ExecuteScalar("SELECT COUNT(*) FROM OriginalLandParcelWithLandOwner");
//            return Convert.ToInt32(result);
//        }

//        /// <summary>
//        /// Maps a DataRow to OriginalLandParcelWithLandOwner entity.
//        /// </summary>
//        private OriginalLandParcelWithLandOwner MapRowToRecord(DataRow row)
//        {
//            return new OriginalLandParcelWithLandOwner
//            {
//                ParcelNo = row["ParcelNo"]?.ToString(),
//                Province = row["Province"]?.ToString(),
//                District = row["District"]?.ToString(),
//                MunicipalityVillage = row["MunicipalityVillage"]?.ToString(),
//                MapSheetNo = row["MapSheetNo"]?.ToString(),
//                LandOwnersName = row["LandOwnersName"]?.ToString(),
//                FatherSpouse = row["FatherSpouse"]?.ToString(),
//                Gender = row["Gender"]?.ToString(),
//                CitizenshipNumber = row["CitizenshipNumber"]?.ToString(),
//                IsTenant = row["IsTenant"]?.ToString(),
//                Address = row["Address"]?.ToString(),
//                LandUse = row["LandUse"]?.ToString(),
//                AreaInSqm = row["AreaInSqm"] != DBNull.Value ? Convert.ToDouble(row["AreaInSqm"]) : null,
//                AreaInRAPD = row["AreaInRAPD"]?.ToString(),
//                AreaInBKD = row["AreaInBKD"]?.ToString(),
//                MothNo = row["MothNo"]?.ToString(),
//                PaanaNo = row["PaanaNo"]?.ToString(),
//                Remarks = row["Remarks"]?.ToString()
//            };
//        }
//    }
//}