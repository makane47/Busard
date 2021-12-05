using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Busard.SqlServer.Tools
{
    internal enum ErrorLogRowType
    {
        Unknown,
        LoginFailed,
        ServerStarted,
        LongIO,
        MemoryPagedOut
    }

    internal struct ErrorLogRow
    {
        public DateTime LogDate;
        public String ProcessInfo;
        public String Text;
    }

    internal struct LoginFailedInfo
    {
        public String User;
        public String Reason;
        public String Client;
    }

    internal struct LongIOInfo
    {
        public uint Occurrences;
        public uint Seconds;
        public String File;
        public String DatabaseName;
        public uint DatabaseId;
    }

    internal struct MemoryPagedOutInfo
    {
        public uint Duration;
        public uint WorkingSet;
        public uint Committed;
        public byte MemoryUtilization;
    }

    /* TODO
     Nonqualified transactions are being rolled back in database xxxx for an Always On Availability Groups state change. Estimated rollback completion: 0%. This is an informational message only. No user action is required.
     SQL Server hosting availability group 'XXXX' did not receive a process event signal from the Windows Server Failover Cluster within the lease timeout period.
     The login packet used to open the connection is structurally invalid; the connection has been closed. Please contact the vendor of the client library. [CLIENT: 10.100.3.14]
     Unable to access availability database 'xxx' because the database replica is not in the PRIMARY or SECONDARY role. Connections to an availability database is permitted only when the database replica is in the PRIMARY or SECONDARY role. Try the operation again later.
     The resumable index 'iiiii' on object '[db].[dbo].[ttttt]' has been paused for '744.25' hours.
     Log was backed up. Database: chro#20140331, creation date(time): 2018/09/04(16:28:16), first LSN: 47370:187:1, last LSN: 47370:238:1, number of dump devices: 1, device information: (FILE=1, TYPE=DISK: {'J:\Program Files\Microsoft SQL Server\MSSQL12.MSSQLSERVER\MSSQL\journaux\chro#20140331_backup_2019_07_02_204503_2518829.trn'}). This is an informational message only. No user action is required.
     BACKUP DATABASE successfully processed 0 pages in 300.232 seconds (0.000 MB/sec).
     I/O is frozen on database xxxx. No user action is required. However, if I/O is not resumed promptly, you could cancel the backup.
     I/O was resumed on database xxxx. No user action is required.
     Setting database option SINGLE_USER to ON for database 'xxxx'.
     The database 'xxxx' is marked RESTORING and is in a state that does not allow recovery to be run.
     */

    internal static class ErrorLogParser
    {
        internal static ErrorLogRowType GetType(ErrorLogRow row)
        {
            if (row.ProcessInfo.Equals("Logon") && row.Text.StartsWith("Login failed")) { return ErrorLogRowType.LoginFailed; }
            if (row.Text.Equals("Starting up database 'tempdb'.")) { return ErrorLogRowType.ServerStarted; }
            if (row.Text.Contains("occurrence(s) of I/O requests taking longer than")) { return ErrorLogRowType.LongIO; }
            if (row.Text.StartsWith("A significant part of sql server process memory has been paged out")) { return ErrorLogRowType.MemoryPagedOut; }
            
            return ErrorLogRowType.Unknown;
        }

        internal static LoginFailedInfo? ParseLoginFailed(ErrorLogRow row)
        {
            if (!row.ProcessInfo.Equals("Logon") || !row.Text.StartsWith("Login failed")) throw new Exception("Not a login failed error log row");

            // Login failed for user 'monitoring_ro'.Reason: Password did not match that for the login provided. [CLIENT: 10.33.0.1]
            var rx = new Regex(@"Login failed for user '(.*)'\.\s?(?:Reason: (.*)\. )?\[CLIENT: ([\d\.]*)\]");
            var result = rx.Match(row.Text);

            if (result.Success)
            {
                var lfi = new LoginFailedInfo
                {
                    User = result.Groups[1].Value,
                    Reason = result.Groups[2].Value,
                    Client = result.Groups[3].Value
                };
                return lfi;
            }
            return null;
        }

        internal static LongIOInfo? ParseLongIO(ErrorLogRow row)
        {
            if (!row.Text.Contains("occurrence(s) of I/O requests taking longer than")) throw new Exception("Not a long IO error log row");

            // SQL Server has encountered 201 occurrence(s) of I/O requests taking longer than 15 seconds to complete on file [T:\MSSQL\DATA\tempdb.mdf] in database id tempdb [2]
            var rx = new Regex(@"SQL Server has encountered (\d*) occurrence\(s\) of I\/O requests taking longer than (\d*) seconds to complete on file \[(.*)\] in database id (.*) \[(\d*)\]");
            var result = rx.Match(row.Text);

            if (result.Success)
            {
                var lii = new LongIOInfo
                {
                    Occurrences = uint.Parse(result.Groups[1].Value),
                    Seconds = uint.Parse(result.Groups[2].Value),
                    File = result.Groups[3].Value,
                    DatabaseName = result.Groups[4].Value,
                    DatabaseId = uint.Parse(result.Groups[5].Value)
                };
                return lii;
            }
            return null;
        }

        internal static MemoryPagedOutInfo? ParseMemoryPagedOut(ErrorLogRow row)
        {
            if (!row.Text.StartsWith("A significant part of sql server process memory has been paged out")) throw new Exception("Not a 'memory paged out' error log row");

            // A significant part of sql server process memory has been paged out. This may result in a performance degradation. Duration: 57632 seconds. Working set (KB): 128580, committed (KB): 358536, memory utilization: 35%.
            var rx = new Regex(@"A significant part of sql server process memory has been paged out. This may result in a performance degradation. Duration: (\d*) seconds. Working set \(KB\): (\d*), committed \(KB\): (\d*), memory utilization: (\d*)\%\.");
            var result = rx.Match(row.Text);

            if (result.Success)
            {
                var mpoi = new MemoryPagedOutInfo
                {
                    Duration = uint.Parse(result.Groups[1].Value),
                    WorkingSet = uint.Parse(result.Groups[2].Value),
                    Committed = uint.Parse(result.Groups[3].Value),
                    MemoryUtilization = byte.Parse(result.Groups[4].Value)
                };
                return mpoi;
            }
            return null;
        }
    }
}
