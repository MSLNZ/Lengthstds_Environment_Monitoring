
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Length_Stds_Environmental_Monitoring
{
    public sealed class MeasurementScheduler
    {
        private readonly List<IMeasurementTask> _tasks =
            new List<IMeasurementTask>();
        private readonly object _lock = new object();
        private CancellationTokenSource _cts;
        private Task _schedulerTask;

        public TimeSpan Interval { get; set; }

        public MeasurementScheduler(TimeSpan initialInterval)
        {
            Interval = initialInterval;
        }


        public void Add(IMeasurementTask task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            lock (_lock)
            {
                var interval = task.Interval > TimeSpan.Zero
                    ? task.Interval
                    : Interval;

                task.Interval = interval;
                task.NextRun = DateTime.UtcNow + interval;

                _tasks.Add(task);
            }
        }


        public void Remove(IMeasurementTask task)
        {
            _tasks.Remove(task);
        }

        public void Start()
        {
            if (_schedulerTask != null)
                return;

            _cts = new CancellationTokenSource();
            _schedulerTask = Task.Run(() => RunAsync(_cts.Token));
        }

        public async Task StopAsync()
        {
            if (_schedulerTask == null)
                return;

            _cts.Cancel();

            try
            {
                await _schedulerTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // expected
            }
            finally
            {
                _cts.Dispose();
                _cts = null;
                _schedulerTask = null;
            }
        }


        private async Task RunAsync(CancellationToken token)
        {
            int index = 0;

            while (!token.IsCancellationRequested)
            {
                var snapshot = _tasks.ToArray();

                if (snapshot.Length == 0)
                {
                    await Task.Delay(Interval, token);
                    continue;
                }

                var task = snapshot[index];

                try
                {
                    task.Device.PrepareSensor(null);
                    task.Device.ResetCycle();
                    task.MeasureOnce();

                    var interval = task.Interval > TimeSpan.Zero
                        ? task.Interval
                        : Interval;

                    task.NextRun = DateTime.UtcNow + interval;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Measurement error: {ex.Message}");
                }

                // ✅ move to next task
                index = (index + 1) % snapshot.Length;

                // ✅ enforce spacing
                await Task.Delay(Interval, token);
            }
        }



        public IReadOnlyList<IMeasurementTask> Tasks =>
            _tasks.AsReadOnly();
    }
}
