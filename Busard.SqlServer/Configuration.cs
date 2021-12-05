using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Data.SqlClient;
using System.Text;
using Tomlyn;
using Tomlyn.Model;

namespace Busard.SqlServer
{
    public static class Configuration
    {
        #region members

        public static bool DebugMode { get; set; }
        public static string Language { get; set; }

        //[Watchers.SqlServer]
        public static string ServerName { get; private set; }
        public static string UserId { get; private set; }
        public static string UserPassword { get; private set; }

        public static SqlConnectionStringBuilder ConnectionString { get; private set; }

        public static Tools.SqlServerMetadata ServerMetadata { get; private set; }

        public static readonly string AppVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();
        
        // for Serilog 
        #endregion

        public static void ReadConfiguration(TomlTable ttable)
        {
            //[Watchers.SqlServer]
            //Configuration.Watchers_SqlServer_watchers = ((TomlArray)(ttable)["watchers"]).OfType<string>();
            //Configuration.ServerName = (string)(ttable)["serverName"];
            //Configuration.UserId = (string)(ttable)["userId"];
            //Configuration.UserPassword = (string)(ttable)["userPassword"];

            //Configuration.ConnectionString = new SqlConnectionStringBuilder(@$"Data Source={Configuration.ServerName};Initial Catalog = master;
            //    User Id={Configuration.UserId}; Password={Configuration.UserPassword};");

            Configuration.ServerMetadata = new Tools.SqlServerMetadata();
        }
}
}
