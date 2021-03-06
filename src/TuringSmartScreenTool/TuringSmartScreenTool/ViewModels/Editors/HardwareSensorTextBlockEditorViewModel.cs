using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Humanizer;
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
using TuringSmartScreenTool.UseCases.Interfaces;
using TuringSmartScreenTool.Views.ContentDialogs.Interdfaces;

namespace TuringSmartScreenTool.ViewModels.Editors
{
    public enum DecimalPlaces
    {
        // TODO: DisplayAttribute
        None,
        One,
        Two,
        Three,
    }

    public class HardwareSensorTextBlockEditorViewModel : BaseTextBlockEditorViewModel
    {
        private readonly ReactiveProperty<string> _sensorId;
        private readonly ReadOnlyReactiveProperty<ISensorInfo> _sensor;
        private readonly ReadOnlyReactiveProperty<double?> _value;
        private readonly IHardwareSelectContentDialog _hardwareSelectContentDialog;

        public override EditorType EditorType => EditorType.HardwareValueText;
        public override ReactiveProperty<string> Name { get; } = new("Hardware Value (Text)");
        public override ReadOnlyReactiveProperty<string> Text { get; }

        public ReadOnlyReactiveProperty<string> SensorName { get; }
        public ReadOnlyReactiveProperty<IEnumerable<string>> UnitCollection { get; }
        public ReactiveProperty<string> Unit { get; } = new();
        public ReactiveProperty<bool> IncludeUnit { get; } = new(true);

        public IEnumerable<DecimalPlaces> DisplayDecimalPlacesCollection { get; } = Enum.GetValues(typeof(DecimalPlaces)).Cast<DecimalPlaces>();
        public ReactiveProperty<DecimalPlaces> DisplayDecimalPlaces { get; } = new(DecimalPlaces.None);

        public ICommand SelectSensorCommand { get; }

        public HardwareSensorTextBlockEditorViewModel(
            IHardwareSelectContentDialog hardwareSelectContentDialog,
            IFindSensorInfoUseCase findSensorInfoUseCase) :
            this(null, hardwareSelectContentDialog, findSensorInfoUseCase)
        { }

        public HardwareSensorTextBlockEditorViewModel(
            string id,
            IHardwareSelectContentDialog hardwareSelectContentDialog,
            IFindSensorInfoUseCase findSensorInfoUseCase)
        {
            _sensorId = new(id);
            _sensor = _sensorId
                .Select(id => findSensorInfoUseCase.Find(id))
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);
            _hardwareSelectContentDialog = hardwareSelectContentDialog;

            _value = _sensor
                .Select(x => x?.Value ?? Observable.Empty<double?>())
                .Switch()
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);

            SensorName = _sensor
                .Select(s =>
                {
                    if (s is null)
                        return "(None)";

                    return s.Parent is not null ?
                        $"{s.Name} ({s.Parent.Name})":
                        $"{s.Name}";
                })
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);

            UnitCollection = _sensor
                .Select(s =>
                {
                    if (s is null)
                        return new string[] { "(None)" };

                    return (IEnumerable<string>)GetUnitCollection(s.ValueUnit);
                })
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);

            UnitCollection
                .Subscribe(collection => Unit.Value = collection.FirstOrDefault())
                .AddTo(_disposables);

            Text =
                Observable.CombineLatest(
                    Unit,
                    IncludeUnit,
                    DisplayDecimalPlaces,
                    _value,
                    (u, iu, f, v) => (unit: u, includeUnit: iu, decimalPlaces: f, value: v))
                .Select(x =>
                {
                    var sensor = _sensor.Value;
                    if (sensor is null || !x.value.HasValue)
                        return "";

                    var units = GetUnitCollection(sensor.ValueUnit);
                    if (!units.Contains(x.unit))
                        return "";

                    var v = x.value.Value;

                    try
                    {
                        return sensor.ValueUnit switch
                        {
                            ValueUnitType.None => getFormatedValue(""),
                            ValueUnitType.Voltage => getFormatedValue(" V"),
                            ValueUnitType.Percentage => getFormatedValue(" %"),
                            ValueUnitType.RotationPerMinute => getFormatedValue(" RPM"),
                            ValueUnitType.LiterPerHour => getFormatedValue(" L/h"),
                            ValueUnitType.Watt => getFormatedValue(" W"),
                            ValueUnitType.Celsius => getCelsiusFormatedValue(),
                            ValueUnitType.Second => getTimeFormatedValue(),
                            ValueUnitType.Byte => getByteFormatedValue(),
                            ValueUnitType.BytePerSecond => getBytePerSecondFormatedValue(),
                            ValueUnitType.Hz => getFrequencyFormatedValue(),
                            _ => throw new InvalidOperationException(),
                        };
                    }
                    catch
                    {
                        return "";
                    }

                    #region Value -> String Methods
                    string getTimeFormatedValue()
                    {
                        var str = v.Seconds().Humanize(culture: CultureInfo.InvariantCulture);

                        // The humanized string always have one space between value and unit.
                        // So split string to remove unit.
                        if (!x.includeUnit)
                        {
                            str = str.Split(" ")[0];
                        }

                        return str;
                    }

                    string getFrequencyFormatedValue()
                    {
                        var format = x.decimalPlaces switch
                        {
                            DecimalPlaces.None => "{0:F0}",
                            DecimalPlaces.One => "{0:F1}",
                            DecimalPlaces.Two => "{0:F2}",
                            DecimalPlaces.Three => "{0:F3}",
                            _ => throw new InvalidOperationException(),
                        };
                        return ValueToStringHelper.ToFrequencyString(v, format, x.includeUnit);
                    }

                    string getCelsiusFormatedValue()
                    {
                        var format = x.decimalPlaces switch
                        {
                            DecimalPlaces.None => "F0",
                            DecimalPlaces.One => "F1",
                            DecimalPlaces.Two => "F2",
                            DecimalPlaces.Three => "F3",
                            _ => throw new InvalidOperationException(),
                        };
                        return ValueToStringHelper.CelsiusToThermalString(
                            v,
                            x.unit == "℃" ? ThermalUnit.Celsius : ThermalUnit.Fahrenheit,
                            format,
                            x.includeUnit);
                    }

                    string getFormatedValue(string unit)
                    {
                        var format = x.decimalPlaces switch
                        {
                            DecimalPlaces.None => "{0:F0}",
                            DecimalPlaces.One => "{0:F1}",
                            DecimalPlaces.Two => "{0:F2}",
                            DecimalPlaces.Three => "{0:F3}",
                            _ => throw new InvalidOperationException(),
                        };
                        return string.Format(format, v) + (x.includeUnit ? unit : "");
                    }

                    string getByteFormatedValue()
                    {
                        var format = x.decimalPlaces switch
                        {
                            DecimalPlaces.None => "0",
                            DecimalPlaces.One => "0.0",
                            DecimalPlaces.Two => "0.00",
                            DecimalPlaces.Three => "0.00",
                            _ => throw new InvalidOperationException(),
                        };
                        var byteSize = ((int)v).Bytes();
                        string str;
                        if (x.unit == "Auto")
                        {
                            str = byteSize.Humanize(format);
                        }
                        else
                        {
                            format += " " + x.unit;
                            str = byteSize.Humanize(format);
                        }

                        // The humanized string always have one space between value and unit.
                        // So split string to remove unit.
                        if (!x.includeUnit)
                        {
                            str = str.Split(" ")[0];
                        }
                        return str;
                    }

                    string getBytePerSecondFormatedValue()
                    {
                        var format = x.decimalPlaces switch
                        {
                            DecimalPlaces.None => "0",
                            DecimalPlaces.One => "0.0",
                            DecimalPlaces.Two => "0.00",
                            DecimalPlaces.Three => "0.00",
                            _ => throw new InvalidOperationException(),
                        };
                        var byteSize = ((int)v).Bytes().Per(TimeSpan.FromSeconds(1));
                        string str;
                        if (x.unit == "Auto")
                        {
                            str = byteSize.Humanize(format);
                        }
                        else
                        {
                            format += " " + x.unit.Replace("/s", "");
                            str = byteSize.Humanize(format);
                        }

                        // The humanized string always have one space between value and unit.
                        // So split string to remove unit.
                        if (!x.includeUnit)
                        {
                            str = str.Split(" ")[0];
                        }
                        return str;
                    }
                    #endregion
                })
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);

            SelectSensorCommand = new RelayCommand(async () =>
            {
                var result = await _hardwareSelectContentDialog.ShowAsync(HardwareSelectType.Sensor);
                if (result == ContentDialogResult.Primary)
                {
                    _sensorId.Value = _hardwareSelectContentDialog.SelectedId;
                }
            });
        }

        private static string[] GetUnitCollection(ValueUnitType type)
        {
            return type switch
            {
                ValueUnitType.None => new string[] { "(None)" },
                ValueUnitType.Voltage => new string[] { "V" },
                ValueUnitType.Percentage => new string[] { "%" },
                ValueUnitType.RotationPerMinute => new string[] { "RPM" },
                ValueUnitType.LiterPerHour => new string[] { "L/h" },
                ValueUnitType.Watt => new string[] { "W" },
                ValueUnitType.Celsius => new string[] { "℃", "℉" },
                ValueUnitType.Second => new string[] { "Auto" },
                ValueUnitType.Byte => new string[] { "Auto", "B", "kB", "MB", "GB", "TB" },
                ValueUnitType.BytePerSecond => new string[] { "Auto", "B/s", "kB/s", "MB/s", "GB/s", "TB/s" },
                ValueUnitType.Hz => new string[] { "Auto", "Hz", "kHz", "MHz", "GHz" },
                _ => throw new InvalidOperationException(),
            };
        }

        #region IEditor
        public class HardwareSensorTextBlockEditorViewModelParameter
        {
            public static readonly string Key = "HardwareSensorText";

            [JsonProperty]
            public string SensorId { get; init; } = null;
            [JsonProperty]
            public string Unit { get; init; } = null;
            [JsonProperty]
            public bool IncludeUnit { get; init; } = true;
            [JsonProperty]
            [JsonConverter(typeof(StringEnumConverter))]
            public DecimalPlaces DisplayDecimalPlaces { get; init; } = DecimalPlaces.None;
        }

        public override async Task<JObject> SaveAsync(SaveAccessory accessory)
        {
            var jobject = await base.SaveAsync(accessory);
            var param = new HardwareSensorTextBlockEditorViewModelParameter()
            {
                SensorId             = _sensorId.Value,
                Unit                 = Unit.Value,
                IncludeUnit          = IncludeUnit.Value,
                DisplayDecimalPlaces = DisplayDecimalPlaces.Value,
            };
            jobject[HardwareSensorTextBlockEditorViewModelParameter.Key] = JToken.FromObject(param);

            return jobject;
        }

        public override async Task LoadAsync(LoadAccessory accessory, JObject jobject)
        {
            await base.LoadAsync(accessory, jobject);

            if (!jobject.TryGetValue(HardwareSensorTextBlockEditorViewModelParameter.Key, out var val))
                return;

            var param = val.ToObject<HardwareSensorTextBlockEditorViewModelParameter>();
            if (param is null)
                return;

            _sensorId.Value = param.SensorId;
            Unit.Value = param.Unit;
            IncludeUnit.Value = param.IncludeUnit;
            DisplayDecimalPlaces.Value = param.DisplayDecimalPlaces;
        }
        #endregion
    }
}
