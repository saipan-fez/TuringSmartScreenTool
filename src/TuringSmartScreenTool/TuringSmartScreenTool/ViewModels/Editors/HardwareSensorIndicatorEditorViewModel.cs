using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Toolkit.Mvvm.Input;
using ModernWpf.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TuringSmartScreenTool.Controllers.Interfaces;
using TuringSmartScreenTool.Entities;
using TuringSmartScreenTool.Helpers;
using TuringSmartScreenTool.Views.ContentDialogs.Interdfaces;

namespace TuringSmartScreenTool.ViewModels.Editors
{
    public enum IndicatorType
    {
        // TODO: DisplayAttribute
        Ring,
        Pie,
        Bar
    }

    public class HardwareSensorIndicatorEditorViewModel : BaseEditorViewModel
    {
        private readonly ReactiveProperty<string> _sensorId;
        private readonly ReadOnlyReactiveProperty<ISensorInfo> _sensor;
        private readonly IHardwareSelectContentDialog _hardwareSelectContentDialog;

        public override ReactiveProperty<string> Name { get; } = new("Hardware Value (Indicator)");
        public override bool IsAutoSizeSupported => false;

        public ReadOnlyReactiveProperty<string> SensorName { get; }
        public ReadOnlyReactiveProperty<double?> Value { get; }
        public ReactiveProperty<double?> Max { get; } = new(100);
        public ReactiveProperty<double?> Min { get; } = new(0);

        public ReactiveProperty<Color> Foreground { get; } = new(Colors.Red);
        public ReactiveProperty<Color> Background { get; } = new(Colors.Blue);
        public ReactiveProperty<bool> IsBackgroundTransparent { get; } = new(false);

        public IEnumerable<IndicatorType> IndicatorCollection { get; } = Enum.GetValues(typeof(IndicatorType)).Cast<IndicatorType>();
        public ReactiveProperty<IndicatorType> Indicator { get; } = new(IndicatorType.Ring);
        public ReadOnlyReactiveProperty<bool> IsRingIndicator { get; }
        public ReactiveProperty<double> IndicatorArcWidth { get; } = new(10);

        public ICommand SelectSensorCommand { get; }
        public ICommand SelectForegroundCommand { get; }
        public ICommand SelectBackgroundCommand { get; }

        public HardwareSensorIndicatorEditorViewModel(
            IHardwareSelectContentDialog hardwareSelectContentDialog,
            // TODO: usecase
            ISensorFinder sensorFinder) :
            this(null, hardwareSelectContentDialog, sensorFinder)
        { }

        public HardwareSensorIndicatorEditorViewModel(
            string id,
            IHardwareSelectContentDialog hardwareSelectContentDialog,
            // TODO: usecase
            ISensorFinder sensorFinder)
        {
            _sensorId = new(id);
            _sensor = _sensorId
                .Select(id => sensorFinder.Find(id))
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);
            _hardwareSelectContentDialog = hardwareSelectContentDialog;

            SensorName = _sensor
                .Select(s =>
                {
                    if (s is null)
                        return "(None)";

                    return s.Parent is not null ?
                        $"{s.Name} ({s.Parent.Name})" :
                        $"{s.Name}";
                })
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);

            Value = _sensor
                .Select(x => x?.Value ?? Observable.Empty<double?>())
                .Switch()
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);

            IsRingIndicator = Indicator
                .Select(x => x == IndicatorType.Ring)
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);

            IsBackgroundTransparent
                .Subscribe(x => Background.Value = Background.Value with { A = (byte)(x ? 0 : 255) })
                .AddTo(_disposables);

            SelectSensorCommand = new RelayCommand(async () =>
            {
                var result = await _hardwareSelectContentDialog.ShowAsync(HardwareSelectType.Sensor);
                if (result == ContentDialogResult.Primary)
                {
                    _sensorId.Value = _hardwareSelectContentDialog.SelectedId;
                }
            });
            SelectForegroundCommand = new RelayCommand(() => SelectColor(Foreground));
            SelectBackgroundCommand = IsBackgroundTransparent
                .Select(x => !x)
                .ToReactiveCommand()
                .WithSubscribe(() => SelectColor(Background))
                .AddTo(_disposables);
        }

        #region IEditor
        public class HardwareSensorIndicatorEditorViewModelParameter
        {
            public static readonly string Key = "HardwareSensorIndicator";

            [JsonProperty]
            public string SensorId { get; init; } = null;
            [JsonProperty]
            public double? Max { get; init; } = 100d;
            [JsonProperty]
            public double? Min { get; init; } = 0d;
            [JsonProperty]
            public string Foreground { get; init; } = ColorHelper.ToString(Colors.Red);
            [JsonProperty]
            public string Background { get; init; } = ColorHelper.ToString(Colors.Blue);
            [JsonProperty]
            public bool IsBackgroundTransparent { get; init; } = false;
            [JsonProperty]
            [JsonConverter(typeof(StringEnumConverter))]
            public IndicatorType Indicator { get; init; } = IndicatorType.Ring;
            [JsonProperty]
            public double IndicatorArcWidth { get; init; } = 10;
        }

        public override async Task<JObject> SaveAsync(SaveAccessory accessory)
        {
            var jobject = await base.SaveAsync(accessory);
            var param = new HardwareSensorIndicatorEditorViewModelParameter()
            {
                SensorId                = _sensorId.Value,
                Max                     = Max.Value,
                Min                     = Min.Value,
                Foreground              = ColorHelper.ToString(Foreground.Value),
                Background              = ColorHelper.ToString(Background.Value),
                IsBackgroundTransparent = IsBackgroundTransparent.Value,
                Indicator               = Indicator.Value,
                IndicatorArcWidth       = IndicatorArcWidth.Value
            };
            jobject[HardwareSensorIndicatorEditorViewModelParameter.Key] = JToken.FromObject(param);

            return jobject;
        }

        public override async Task LoadAsync(LoadAccessory accessory, JObject jobject)
        {
            await base.LoadAsync(accessory, jobject);

            if (!jobject.TryGetValue(HardwareSensorIndicatorEditorViewModelParameter.Key, out var val))
                return;

            var param = val.ToObject<HardwareSensorIndicatorEditorViewModelParameter>();
            if (param is null)
                return;

            _sensorId.Value               = param.SensorId;
            Max.Value                     = param.Max;
            Min.Value                     = param.Min;
            Foreground.Value              = ColorHelper.FromString(param.Foreground);
            Background.Value              = ColorHelper.FromString(param.Background);
            IsBackgroundTransparent.Value = param.IsBackgroundTransparent;
            Indicator.Value               = param.Indicator;
            IndicatorArcWidth.Value       = param.IndicatorArcWidth;
        }
        #endregion
    }
}
