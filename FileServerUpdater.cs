using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Length_Stds_Environmental_Monitoring
{
    public sealed class FileServerUpdater
    {
        private readonly string _localRoot;
        private readonly string _serverRoot;
        private readonly TimeSpan _interval;

        private CancellationTokenSource _cts;
        private Task _task;

        public FileServerUpdater(string localRoot, string serverRoot, TimeSpan interval)
        {
            _localRoot = localRoot;
            _serverRoot = serverRoot;
            _interval = interval;
        }

        public void Start()
        {
            if (_task != null)
                return;

            _cts = new CancellationTokenSource();
            _task = Task.Run(() => RunAsync(_cts.Token));
        }

        public async Task StopAsync()
        {
            if (_task == null)
                return;

            _cts.Cancel();

            try
            {
                await _task;
            }
            catch (OperationCanceledException) { }

            _task = null;
            _cts.Dispose();
        }

        private async Task RunAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                SyncDirectories();

                await Task.Delay(_interval, token);
            }
        }

        private void SyncDirectories()
        {
            if (!Directory.Exists(_localRoot))
                return;

            foreach (var file in Directory.GetFiles(_localRoot, "*.*", SearchOption.AllDirectories))
            {
                string relativePath = Path.GetRelativePath(_localRoot, file);
                string destPath = Path.Combine(_serverRoot, relativePath);

                string destDir = Path.GetDirectoryName(destPath);
                if (!Directory.Exists(destDir))
                    Directory.CreateDirectory(destDir);

                try
                {
                    File.Copy(file, destPath, overwrite: true);
                }
                catch (IOException)
                {
                    // File in use — skip
                }
            }
        }
    }
}
