using System.Threading.Tasks;

namespace Busard.Core.Monitoring
{
    public interface ITimedWatcherService
    {
        public string Name { get; }
        Task RunAsync();
    }
}