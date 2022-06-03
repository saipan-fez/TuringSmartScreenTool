using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Reactive.Bindings;
using TuringSmartScreenTool.Controllers.Interfaces;
using TuringSmartScreenTool.Entities;

namespace TuringSmartScreenTool.Controllers
{
    public class TimeParameter
    {
        public TimeSpan Interval { get; init; } = TimeSpan.FromMilliseconds(100);
    }

    public class TimeManager : IDisposable, ITimeManager
    {
        private class TimeData : ITimeData
        {
            private ReactiveProperty<DateTimeOffset> _value;

            public IObservable<DateTimeOffset> Value => _value;

            public TimeData(DateTimeOffset dateTimeOffset)
            {
                _value = new(dateTimeOffset);
            }

            public void Update(DateTimeOffset dateTimeOffset)
            {
                _value.Value = dateTimeOffset;
            }
        }

        private readonly ILogger<TimeManager> _logger;
        private readonly IValueUpdateManager _valueUpdateManager;
        private readonly TimeParameter _parameter;

        private readonly TimeData _timeData = new(DateTimeOffset.UtcNow);
        private string _id = null;

        public TimeManager(
            ILogger<TimeManager> logger,
            IValueUpdateManager valueUpdateManager,
            IOptions<TimeParameter> parameter)
        {
            _logger = logger;
            _valueUpdateManager = valueUpdateManager;
            _parameter = parameter.Value;
        }

        public void Dispose()
        {
            if (_id is not null)
                _valueUpdateManager.Unregister(_id);
        }

        public ITimeData Get()
        {
            if (_id is null)
            {
                _id = _valueUpdateManager.Register(
                    nameof(TimeManager),
                    _parameter.Interval,
                    UpdateTime);
            }

            return _timeData;
        }

        private void UpdateTime()
        {
            _timeData.Update(DateTimeOffset.UtcNow);
        }
    }
}
