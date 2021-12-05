using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
//using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Busard.Watcher
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Orchestrator.CreateHostBuilder(args).Build().Run();
        }
    }
}
