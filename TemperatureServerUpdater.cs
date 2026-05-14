using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Length_Stds_Environmental_Monitoring
{
    public sealed class TemperatureServerUpdater
    {
        private readonly string _localRoot;
        private readonly string _serverRoot;
        private readonly TimeSpan _syncInterval;

        private CancellationTokenSource _cts;
        private Task _task;

        public TemperatureServerUpdater(
            string localRoot,
            string serverRoot,
            TimeSpan syncInterval)
        {
            _localRoot = localRoot;
            _serverRoot = serverRoot;
            _syncInterval = syncInterval;
        }

        public void Start()
        {
            if (_task != null && !_task.IsCompleted)
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
                await _task.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // expected
            }
            finally
            {
                _cts.Dispose();
                _cts = null;
                _task = null;
            }
        }

        private async Task RunAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(_syncInterval, token);
                SyncOnce();
            }
        }


        private void SyncOnce()
        {
            if (!Directory.Exists(_localRoot))
                return;

            foreach (var localFile in Directory.EnumerateFiles(
                         _localRoot, "*.txt", SearchOption.AllDirectories))
            {
                try
                {
                    var relativePath = Path.GetRelativePath(_localRoot, localFile);
                    var serverFile = Path.Combine(_serverRoot, relativePath);

                    SyncFileIncrementally(localFile, serverFile);
                }
                catch (IOException)
                {
                    // Server unavailable or file locked → retry later
                }
                catch (UnauthorizedAccessException)
                {
                    // Permission issue → retry later
                }
            }
        }

        private void SyncFileIncrementally(string localFile, string serverFile)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(serverFile)!);

            if (!File.Exists(serverFile))
            {
                // First time: copy entire file
                File.Copy(localFile, serverFile);
                return;
            }

            long localLength = new FileInfo(localFile).Length;
            long serverLength = new FileInfo(serverFile).Length;

            if (serverLength == localLength)
            {
                // Already in sync
                return;
            }

            if (serverLength > localLength)
            {
                // Unexpected: server file is longer than local
                // Log this and skip to avoid corruption
                return;
            }

            // Append only new bytes
            using (var localStream = new FileStream(
                localFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var serverStream = new FileStream(
                serverFile, FileMode.Append, FileAccess.Write, FileShare.Read))
            {
                localStream.Seek(serverLength, SeekOrigin.Begin);
                localStream.CopyTo(serverStream);
            }
        }


    }
}