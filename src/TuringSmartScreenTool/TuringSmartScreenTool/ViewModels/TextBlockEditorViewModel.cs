using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Toolkit.Mvvm.Input;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace TuringSmartScreenTool.ViewModels
{
    // HW意外に取得するもの
    //   天気
    //   日時

    public enum MonitorType
    {
        // Motherboard
        MotherboardName,
        MotherboardTemperature,

        // CPU
        CpuName,
        CpuUsageRate,
        CpuClock,
        CpuPackagePower,
        CpuTemperature,
        CpuFanName,

        // FAN
        GenericFanName,
        GenericFanSpeed,

        // Memory
        Memory, // (free/usage/total)
        MemoryClock,

        // GPU
        GpuName,
        GpuClock,
        GpuUsageRate,
        GpuMemoryControllerUsage,
        GpuVram, // (free/usage/total)
        GpuTemperature,
        GpuFanSpeed,
        GpuPower,

        // Disk
        StorageName,
        StorageUsageRate,
        StorageTotalSize,
        StorageUsageSize,
        StorageTemperature,

        // Network
        NetworkAdapterName,
        NetworkAdapterIPv4Address,
        NetworkAdapterIPv6Address,
        NetworkAdapterReceive,
        NetworkAdapterSend,
    }
    // sigleton
    public class HardwareMintorFactory
    {
        private PerformanceCounter cpuCounter = new PerformanceCounter();

        public HardwareInfoRepository()
        {
        }
    }

    public class CpuUsageTextBlockEditorViewModel : AutoUpdatableTextBlockEditorViewModel
    {
        private static readonly string[] Units = { "%", "None" };

        public override ObservableCollection<string> UnitCollection { get; } = new(Units);
        public override ReactiveProperty<string> Unit { get; } = new(Units[0]);

        public CpuUsageTextBlockEditorViewModel(TimeSpan interval, Func<double> valueUpdateFunc)
            : base(interval, valueUpdateFunc)
        {
        }

    }

    public abstract class AutoUpdatableTextBlockEditorViewModel : TextBlockEditorViewModel
    {
        private readonly Func<double> _valueUpdateFunc;

        public abstract ObservableCollection<string> UnitCollection { get; }
        public abstract ReactiveProperty<string> Unit { get; }

        protected ReactiveProperty<string> ValueFormat { get; } = new();
        protected ReactiveProperty<double?> Value { get; } = new((double?)null);

        public AutoUpdatableTextBlockEditorViewModel(TimeSpan interval, Func<double> valueUpdateFunc)
        {
            _valueUpdateFunc = valueUpdateFunc;

            // register to update Text property
            Observable.CombineLatest(
                Unit,
                ValueFormat,
                Value,
                (u, f, v) => (unit: u, format: f, value: v))
            .Select(x => $"{string.Format(x.format, x.value)}{x.unit}")
            .Subscribe(x => Text.Value = x)
            .AddTo(_disposables);
        }
    }

    public class TextBlockEditorViewModel : CommonEditorViewModel
    {
        public record FontWeightData(string Name, FontWeight Weight);

        private static readonly List<FontWeightData> FontWeightDataList = new()
        {
            new FontWeightData("ExtraBold", FontWeights.ExtraBold),
            new FontWeightData("Bold", FontWeights.Bold),
            new FontWeightData("Normal", FontWeights.Normal),
            new FontWeightData("Light", FontWeights.Light),
            new FontWeightData("Thin", FontWeights.Thin),
        };

        public override ReactiveProperty<string> Name { get; } = new("TextBlock");

        public ReactiveProperty<double> FontSize { get; } = new(12);

        public IReadOnlyCollection<FontWeightData> FontWeightDataCollection { get; } = FontWeightDataList;
        public ReactiveProperty<FontWeightData> SelectedFontWeightData { get; } = new(FontWeightDataList.FirstOrDefault(x => x.Name == "Normal"));
        public ReadOnlyReactiveProperty<FontWeight> SelectedFontWeight { get; }

        public ICollection<FontFamily> FontFamilyCollection { get; } = GetFontFamilyCollection();
        public ReactiveProperty<FontFamily> SelectedFontFamily { get; } = new(GetSystemDefaultFont());

        public ReactiveProperty<Color> Foreground { get; } = new(Colors.White);

        public ICommand SelectForegroundCommand { get; }

        public ReactiveProperty<string> Text { get; } = new("Text");

        public TextBlockEditorViewModel()
        {
            SelectedFontWeight = SelectedFontWeightData
                .Select(x => x.Weight)
                .ToReadOnlyReactiveProperty();

            SelectForegroundCommand = new RelayCommand(() => SelectColor(Foreground));
        }

        private static ICollection<FontFamily> GetFontFamilyCollection()
        {
            return Fonts.SystemFontFamilies;
        }

        private static FontFamily GetSystemDefaultFont()
        {
            var fonts = GetFontFamilyCollection();
            var defaultFont = new FontFamily(System.Drawing.SystemFonts.DefaultFont.FontFamily.Name);
            return fonts.Contains(defaultFont) ? defaultFont : fonts.FirstOrDefault();
        }
    }
}
