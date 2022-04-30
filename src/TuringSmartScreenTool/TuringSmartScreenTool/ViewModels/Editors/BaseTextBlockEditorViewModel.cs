using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Toolkit.Mvvm.Input;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace TuringSmartScreenTool.ViewModels.Editors
{
    public abstract class BaseTextBlockEditorViewModel : BaseEditorViewModel
    {
        public record FontWeightData(string Name, FontWeight Weight);

        private static readonly List<FontWeightData> s_fontWeightDataList = new()
        {
            new FontWeightData("ExtraBold", FontWeights.ExtraBold),
            new FontWeightData("Bold", FontWeights.Bold),
            new FontWeightData("Normal", FontWeights.Normal),
            new FontWeightData("Light", FontWeights.Light),
            new FontWeightData("Thin", FontWeights.Thin),
        };

        public override ReactiveProperty<string> Name { get; } = new("TextBlock");

        public ReactiveProperty<double> FontSize { get; } = new(12);

        public IReadOnlyCollection<FontWeightData> FontWeightDataCollection { get; } = s_fontWeightDataList;
        public ReactiveProperty<FontWeightData> SelectedFontWeightData { get; } = new(s_fontWeightDataList.FirstOrDefault(x => x.Name == "Normal"));
        public ReadOnlyReactiveProperty<FontWeight> SelectedFontWeight { get; }

        public ICollection<FontFamily> FontFamilyCollection { get; } = GetFontFamilyCollection();
        public ReactiveProperty<FontFamily> SelectedFontFamily { get; } = new(GetSystemDefaultFont());

        public ReactiveProperty<Color> Foreground { get; } = new(Colors.White);

        public ICommand SelectForegroundCommand { get; }

        public abstract IReadOnlyReactiveProperty<string> Text { get; }

        public BaseTextBlockEditorViewModel()
        {
            SelectedFontWeight = SelectedFontWeightData
                .Select(x => x.Weight)
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);

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
