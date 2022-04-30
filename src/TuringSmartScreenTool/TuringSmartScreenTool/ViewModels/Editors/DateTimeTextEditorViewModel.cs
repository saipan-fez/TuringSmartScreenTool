using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TuringSmartScreenTool.Controllers;

namespace TuringSmartScreenTool.ViewModels.Editors
{
    public record DateTimeFormat(string DisplayName, string Format);

    public class DateTimeTextEditorViewModel : BaseTextBlockEditorViewModel
    {
        private static readonly DateTimeFormat[] s_dateTimeFormatCollection = new DateTimeFormat[]
        {
            new DateTimeFormat("Date (Short)", "d"),
            new DateTimeFormat("Date (Long)", "D"),
            new DateTimeFormat("Full DateTime (Short)", "f"),
            new DateTimeFormat("Full DateTime (Long)", "F"),
            new DateTimeFormat("General DateTime (Short)", "g"),
            new DateTimeFormat("General DateTime (Long)", "G"),
            new DateTimeFormat("Time (Short)", "t"),
            new DateTimeFormat("Time (Long)", "T"),
            new DateTimeFormat("Day/Month", "m"),
            new DateTimeFormat("Month/Year", "Y"),
            new DateTimeFormat("yyyy/MM/dd", "yyyy/MM/dd"),
            new DateTimeFormat("MM/dd/yyyy", "MM/dd/yyyy"),
            new DateTimeFormat("HH:mm", "HH:mm"),
            new DateTimeFormat("HH:mm:ss", "HH:mm:ss"),
        };

        public override ReactiveProperty<string> Name { get; } = new("DateTime");
        public override ReadOnlyReactiveProperty<string> Text { get; }

        public IEnumerable<TimeZoneInfo> TimeZoneInfoCollection { get; } = TimeZoneInfo.GetSystemTimeZones();
        public ReactiveProperty<TimeZoneInfo> SelectedTimeZoneInfo { get; } = new(TimeZoneInfo.Local);

        public IEnumerable<DateTimeFormat> DateTimeFormatCollection { get; } = s_dateTimeFormatCollection;
        public ReactiveProperty<DateTimeFormat> SelectedDateTimeFormat { get; } = new(s_dateTimeFormatCollection[0]);

        public DateTimeTextEditorViewModel(
            // TODO: usecase
            ITimeManager timeManager)
        {
            var timeInfo = timeManager.Get();

            Text =
                Observable.CombineLatest(
                    timeInfo.Value,
                    SelectedTimeZoneInfo,
                    SelectedDateTimeFormat,
                    (d, tz, f) => (dateTimeOffset: d, timeZoneInfo: tz, dateTimeFormat: f))
                .Select(x => GetDateTimeText(x.dateTimeOffset, x.timeZoneInfo, x.dateTimeFormat.Format))
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);
        }

        private static string GetDateTimeText(DateTimeOffset dateTimeOffset, TimeZoneInfo timeZoneInfo, string dateTimeFormat)
        {
            var time = TimeZoneInfo.ConvertTime(dateTimeOffset, timeZoneInfo);
            return time.ToString(dateTimeFormat);
        }
    }
}
