using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Busard.Core.Notification
{
    public class SqlServerTableNotifier : NotifierBase
    {
        private readonly SqlConnectionStringBuilder _connectionString;
        private readonly SqlServerTableNotificationConfiguration _config;

        public SqlServerTableNotifier(SqlServerTableNotificationConfiguration config)
        {
            _config = config;
            this._connectionString = new SqlConnectionStringBuilder(@$"Data Source={_config.ServerName};Initial Catalog = master;
                User Id={_config.UserId}; Password={_config.UserPassword};");
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        protected override async Task SendNotification()
        {
            var sql = $"INSERT INTO {_config.TableName} (When, Message) VALUES";

            sql += String.Join(',', _messages.Select(m => $"(CURRENT_TIMESTAMP, '{m}')"));

            using var cn = new SqlConnection(this._connectionString.ConnectionString);
            using (var cmd = new SqlCommand(sql, cn))
            {
                cn.Open();
                await cmd.ExecuteNonQueryAsync();
            }
            cn.Close();

        }
    }
}
