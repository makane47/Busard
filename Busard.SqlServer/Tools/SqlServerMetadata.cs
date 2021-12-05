using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Busard.SqlServer.Tools
{
    public struct DatabaseInfo
    {
        public string Name;
        public int DatabaseId;
        public byte CompatibilityLevel;
        public string CollationName;
        public bool IsReadOnly;
        public bool IsAutoCloseOn;
        public bool IsAutoShrinkOn;
        public string State;
        public string RecoveryModel;
        public bool IsAutoCreateStatsOn;
        public bool IsAutoUpdateStatsOn;
        //public bool IsQueryStorOn;
        public string LogReuseWait;
    }

    public class SqlServerMetadata
    {
        public string ComputerName { get; private set; }
        public string InstanceName { get; private set; }
        public string Edition { get; private set; }
        public string ProductVersion { get; private set; }
        public string ProductLevel { get; private set; }

        public Dictionary<int, DatabaseInfo> Databases { get; private set; }

        public SqlServerMetadata()
        {
            this.GetServerMetadata();
            if (ushort.Parse(ProductVersion.Split('.')[0]) < 11) // we need to be at least on SQL Server 2012
            {
                throw new Exception(Resources.Strings.NeedSQLServer2012);
            }
            this.GetDatabasesList();
        }

        private void GetServerMetadata()
        {
            using var cn = new SqlConnection(Configuration.ConnectionString.ConnectionString);
            using (var cmd = new SqlCommand(Resources.Queries.GetServerMetadata, cn))
            {
                cn.Open();
                using var reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                if (reader.Read())
                {
                    this.ComputerName = reader["ComputerName"].ToString();
                    this.InstanceName = reader["InstanceName"].ToString();
                    this.Edition = reader["Edition"].ToString();
                    this.ProductVersion = reader["ProductVersion"].ToString();
                    this.ComputerName = reader["ProductLevel"].ToString();
                }
            }
            cn.Close();

        }
        private void GetDatabasesList()
        {
            try
            {
                using (var cn = new SqlConnection(Configuration.ConnectionString.ConnectionString))
                {
                    using var cmd = new SqlCommand(Resources.Queries.GetDatabasesList, cn);
                    cn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        Databases = new Dictionary<int, DatabaseInfo>();
                        while (reader.Read())
                        {
                            var dbinfo = new DatabaseInfo()
                            {
                                Name = reader["name"].ToString(),
                                DatabaseId = reader.GetInt32("database_id"),
                                CompatibilityLevel = reader.GetByte("compatibility_level"),
                                CollationName = reader["collation_name"].ToString(),
                                IsReadOnly = reader.GetBoolean("is_read_only"),
                                IsAutoCloseOn = reader.GetBoolean("is_auto_close_on"),
                                IsAutoShrinkOn = reader.GetBoolean("is_auto_shrink_on"),
                                State = reader["state_desc"].ToString(),
                                RecoveryModel = reader["recovery_model_desc"].ToString(),
                                IsAutoCreateStatsOn = reader.GetBoolean("is_auto_create_stats_on"),
                                IsAutoUpdateStatsOn = reader.GetBoolean("is_auto_update_stats_on"),
                                LogReuseWait = reader["log_reuse_wait_desc"].ToString()
                            };
                            Databases.Add(reader.GetInt32("database_id"), dbinfo);
                        }
                    }
                    cn.Close();
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
