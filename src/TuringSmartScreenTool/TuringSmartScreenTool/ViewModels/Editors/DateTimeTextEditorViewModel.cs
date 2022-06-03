using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TuringSmartScreenTool.Entities;
using TuringSmartScreenTool.UseCases.Interfaces;

namespace TuringSmartScreenTool.ViewModels.Editors
{
    public record DateTimeFormat(string DisplayName, string Format);

    public class DateTimeTextEditorViewModel : BaseTextBlockEditorViewModel
    {
        private static readonly TimeZoneInfo[] s_timeZoneInfoCollection = TimeZoneInfo.GetSystemTimeZones().ToArray();
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

        public override EditorType EditorType => EditorType.DateTime;
        public override ReactiveProperty<string> Name { get; } = new("DateTime");
        public override ReadOnlyReactiveProperty<string> Text { get; }

        public IEnumerable<TimeZoneInfo> TimeZoneInfoCollection { get; } = s_timeZoneInfoCollection;
        public ReactiveProperty<TimeZoneInfo> SelectedTimeZoneInfo { get; } = new(TimeZoneInfo.Local);

        public IEnumerable<DateTimeFormat> DateTimeFormatCollection { get; } = s_dateTimeFormatCollection;
        public ReactiveProperty<DateTimeFormat> SelectedDateTimeFormat { get; } = new(s_dateTimeFormatCollection[0]);

        public DateTimeTextEditorViewModel(
            IGetTimeDataUseCase getTimeDataUseCase)
        {
            var timeData = getTimeDataUseCase.Get();

            Text =
                Observable.CombineLatest(
                    timeData.Value,
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

        #region IEditor
        public class DateTimeTextEditorViewModelParameter
        {
            public static readonly string Key = "DateTimeText";

            [JsonProperty]
            public string SelectedTimeZoneInfo { get; init; } = TimeZoneInfo.Local.Id;
            [JsonProperty]
            public string SelectedDateTimeFormat { get; init; } = s_dateTimeFormatCollection[0].Format;
        }

        public override async Task<JObject> SaveAsync(SaveAccessory accessory)
        {
            var jobject = await base.SaveAsync(accessory);
            var param = new DateTimeTextEditorViewModelParameter()
            {
                SelectedTimeZoneInfo   = SelectedTimeZoneInfo.Value.Id,
                SelectedDateTimeFormat = SelectedDateTimeFormat.Value.Format
            };
            jobject[DateTimeTextEditorViewModelParameter.Key] = JToken.FromObject(param);

            return jobject;
        }

        public override async Task LoadAsync(LoadAccessory accessory, JObject jobject)
        {
            await base.LoadAsync(accessory, jobject);

            if (!jobject.TryGetValue(DateTimeTextEditorViewModelParameter.Key, out var val))
                return;

            var param = val.ToObject<DateTimeTextEditorViewModelParameter>();
            if (param is null)
                return;

            SelectedTimeZoneInfo.Value = s_timeZoneInfoCollection.FirstOrDefault(x => x.Id == param.SelectedTimeZoneInfo) ?? TimeZoneInfo.Local;
            SelectedDateTimeFormat.Value = s_dateTimeFormatCollection.FirstOrDefault(x => x.Format == param.SelectedDateTimeFormat) ?? s_dateTimeFormatCollection[0];
        }
        #endregion
    }
}
