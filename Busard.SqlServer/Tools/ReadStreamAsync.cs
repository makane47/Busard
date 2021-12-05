using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SqlServer.XEvent.XELite;
using Serilog;

namespace Busard.SqlServer.Tools
{
    public class ReadXEventStream: IDisposable
    {
        public readonly string ConnectionString;
        public readonly string SessionName;
        private XELiveEventStreamer _xeStream;

        public ReadXEventStream(string connectionString, string sessionName)
        {
            this.ConnectionString = connectionString;
            this.SessionName = sessionName;
            _xeStream = new XELiveEventStreamer(this.ConnectionString, this.SessionName);
        }

        public async Task ReadStreamAsync(Action<IXEvent> callback, CancellationToken stoppingToken)
        {
            var cancellationTokenSource = new CancellationTokenSource();

            Task readTask = _xeStream.ReadEventStream(() =>
            {
                Log.Information($"ReadXEventStream connected to session [{this.SessionName}]");
                return Task.CompletedTask;
            },
                xevent =>
                {
                    callback(xevent);
                    return Task.CompletedTask;
                },
                cancellationTokenSource.Token);


            try
            {
                //Task.WaitAny(waitTask, readTask);
                //await Task.WhenAny(readTask);
                await readTask;
            }
            catch (TaskCanceledException)
            {
                Log.Information($"Session {this.SessionName} stopped on TaskCanceledException");
            }
            catch (Exception e)
            {
                Log.Error($"Session {this.SessionName} failed with: {e.Message}");
                throw;
            }

            if (readTask.IsFaulted)
            {
                Log.Error($"Session {this.SessionName} IsFaulted with", readTask.Exception);
            }
            
            Log.Debug($"Session {this.SessionName} stopped reading.");

        }

        #region IDisposable Support
        private bool disposedValue = false; // Pour détecter les appels redondants

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: supprimer l'état managé (objets managés).
                    // _xeStream.
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

    }
}
