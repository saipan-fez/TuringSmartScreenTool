using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TuringSmartScreenTool.Controllers.Interfaces;

namespace TuringSmartScreenTool.Controllers
{
    public record ValueUpdateParameter
    {
        public TimeSpan Interval { get; } = TimeSpan.FromMilliseconds(100);
    }

    public class ValueUpdateManager : IDisposable, IValueUpdateManager
    {
        private class UpdateTask
        {
            public string Name { get; init; }
            public TimeSpan Interval { get; init; }
            public Action Task { get; init; }
            public Func<CancellationToken, Task> AsyncTask { get; init; }
            public DateTime NextUpdate { get; set; }
        }

        private readonly ILogger<ValueUpdateManager> _logger;
        private readonly ValueUpdateParameter _parameter;
        private readonly ConcurrentDictionary<string, UpdateTask> UpdateTaskDictionary = new();

        private CancellationTokenSource _cts;
        private Task _task;

        public ValueUpdateManager(
            ILogger<ValueUpdateManager> logger,
            IOptions<ValueUpdateParameter> parameter)
        {
            _logger = logger;
            _parameter = parameter.Value;

            Start();
        }

        public void Dispose()
        {
            try
            {
                Stop();
            }
            catch
            { }
        }

        public string Register(string name, TimeSpan interval, Action updateAction)
        {
            if (interval < TimeSpan.FromMilliseconds(100))
            {
                _logger.LogWarning("interval is higher resolution than DateTime class.");
            }

            var newId = Guid.NewGuid().ToString();

            UpdateTaskDictionary.TryAdd(
                newId,
                new UpdateTask
                {
                    Name = name,
                    Interval = interval,
                    NextUpdate = DateTime.Now.Add(interval),
                    Task = updateAction
                });

            return newId;
        }

        public string Register(string name, TimeSpan interval, Func<CancellationToken, Task> updateAsyncFunction)
        {
            if (interval < TimeSpan.FromMilliseconds(100))
            {
                _logger.LogWarning("interval is higher resolution than DateTime class.");
            }

            var newId = Guid.NewGuid().ToString();

            UpdateTaskDictionary.TryAdd(
                newId,
                new UpdateTask
                {
                    Name = name,
                    Interval = interval,
                    NextUpdate = DateTime.Now.AddSeconds(-1),   // Execute the first execution in the next loop
                    AsyncTask = updateAsyncFunction
                });

            return newId;
        }

        public void Unregister(string id)
        {
            UpdateTaskDictionary.Remove(id, out _);
        }

        private void Start()
        {
            if (_cts is not null && _task is not null)
                return;

            var factory = new TaskFactory();
            _cts = new CancellationTokenSource();
            _task = factory.StartNew(() =>
            {
                var stopwatch = new Stopwatch();
                var token = _cts.Token;
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        stopwatch.Restart();
                        var start = stopwatch.Elapsed;

                        //foreach (var pair in UpdateTaskDictionary)
                        Parallel.ForEach(UpdateTaskDictionary, async (pair) =>
                        {
                            var v = pair.Value;
                            if (v.NextUpdate > DateTime.Now)
                                return;

                            try
                            {
                                if (v.Task is not null)
                                    v.Task();
                                if (v.AsyncTask is not null)
                                    await v.AsyncTask(token);

                                _logger.LogTrace("task executed. name:{name} next:{next}", v.Name, v.NextUpdate);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "task errored. name:{name} next:{next}", v.Name, v.NextUpdate);
                            }
                            finally
                            {
                                v.NextUpdate = DateTime.Now.Add(v.Interval);
                            }
                        });

                        var elapsed = stopwatch.Elapsed - start;
                        var waitTime = _parameter.Interval - elapsed;
                        if (waitTime > TimeSpan.Zero)
                        {
                            Thread.Sleep(waitTime);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Value update thread has errored.");
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }

        private void Stop()
        {
            try
            {
                _cts?.Cancel();
                _task?.Wait();
            }
            catch
            { }
            finally
            {
                try
                {
                    _cts?.Dispose();
                    _task?.Dispose();
                }
                catch
                { }

                _cts = null;
                _task = null;
            }
        }
    }
}
