using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Busard.SqlServer.Tools
{
    internal struct Process
    {
        public string InputBuffer { get; set; }
        public string ClientApp { get; set; }
        public string HostName { get; set; }
        public string LoginName { get; set; }
        public string CurrentDb { get; set; }
        public DateTime LastBatchStarted { get; set; }
    }

    internal struct BlockedProcessReport
    {
        public string WaitResource { get; set; }
        public ulong WaitTime { get; set; }
        public Process BlockedProcess { get; set; } //= new Process();
        public Process BlockingProcess { get; set; } //= new Process();

    }

    internal class BlockedProcessReportReader
    {
        public readonly XElement XMLBlockedProcessReport;
        public BlockedProcessReport BlockedProcessReport { get; private set; }

        private const string _testXml = @"<blocked-process-report monitorLoop=""639910"">
                                         <blocked-process>
                                          <process id = ""processfa500a7848"" taskpriority=""0"" logused=""0"" waitresource=""RID: 2:1:40848:0"" waittime=""19317"" ownerId=""104763099"" transactionname=""SELECT"" lasttranstarted=""2020-05-16T03:25:29.400"" XDES=""0xfa14be5ac0"" lockMode=""S"" schedulerid=""2"" kpid=""1792"" status=""suspended"" spid=""85"" sbid=""0"" ecid=""0"" priority=""0"" trancount=""0"" lastbatchstarted=""2020-05-16T03:25:29.403"" lastbatchcompleted=""2020-05-16T03:25:29.323"" lastattention=""1900-01-01T00:00:00.323"" clientapp=""Microsoft SQL Server Management Studio - Requête"" hostname=""TARDIS-RUDI"" hostpid=""19552"" loginname=""rudi_admin"" isolationlevel=""read committed (2)"" xactid=""104763099"" currentdb=""1"" lockTimeout=""4294967295"" clientoption1=""671098976"" clientoption2=""390200"">
                                           <executionStack>
                                            <frame line = ""1"" stmtend=""38"" sqlhandle=""0x02000000359b2124ca0e8cfef2ee88ad012c9cddaa3067db0000000000000000000000000000000000000000"" />
                                           </executionStack>
                                           <inputbuf>
                                        SELECT* FROM ##test   </inputbuf>
                                          </process>
                                         </blocked-process>
                                         <blocking-process>
                                          <process status = ""sleeping"" spid= ""76"" sbid= ""0"" ecid= ""0"" priority= ""0"" trancount= ""1"" lastbatchstarted= ""2020-05-16T03:25:12.800"" lastbatchcompleted= ""2020-05-16T03:25:12.800"" lastattention= ""1900-01-01T00:00:00.800"" clientapp= ""Microsoft SQL Server Management Studio - Requête"" hostname= ""TARDIS-RUDI"" hostpid= ""19552"" loginname= ""rudi_admin"" isolationlevel= ""read committed (2)"" xactid= ""104738094"" currentdb= ""1"" lockTimeout= ""4294967295"" clientoption1= ""671098976"" clientoption2= ""390200"" >
                                           < executionStack />
                                           < inputbuf >
                                        SELECT @@trancount</inputbuf>
                                          </process>
                                         </blocking-process>
                                        </blocked-process-report>";

        public BlockedProcessReportReader(string xml)
        {
            this.XMLBlockedProcessReport = XElement.Parse(xml);
        }

        private void ParseReport()
        {
            this.BlockedProcessReport = new BlockedProcessReport()
            {
                WaitTime = ulong.Parse(XMLBlockedProcessReport.Elements("blocked-process").Elements("process").First().Attributes("waittime").First().Value),
                WaitResource = XMLBlockedProcessReport.Elements("blocked-process").Elements("process").First().Attributes("waitresource").First().Value,
                BlockedProcess = new Process()
                {
                    InputBuffer = XMLBlockedProcessReport.Elements("blocked-process").Elements("process").First().Elements("inputbuf").First().Value.Trim(),
                    LastBatchStarted = DateTime.Parse(XMLBlockedProcessReport.Elements("blocked-process").Elements("process").First().Attributes("lastbatchstarted").First().Value)
                },
                BlockingProcess = new Process()
                {
                    InputBuffer = XMLBlockedProcessReport.Elements("blocking-process").Elements("process").First().Elements("inputbuf").First().Value.Trim(),
                    LastBatchStarted = DateTime.Parse(XMLBlockedProcessReport.Elements("blocking-process").Elements("process").First().Attributes("lastbatchstarted").First().Value)
                }
            };
        }
    }

}
