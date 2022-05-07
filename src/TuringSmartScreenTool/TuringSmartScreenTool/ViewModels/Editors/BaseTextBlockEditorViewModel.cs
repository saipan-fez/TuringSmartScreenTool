using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Toolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TuringSmartScreenTool.Entities;
using TuringSmartScreenTool.Helpers;

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

        public virtual IReadOnlyReactiveProperty<bool> CanSelectFontSize { get; } = new ReactiveProperty<bool>(true);
        public ReactiveProperty<double> FontSize { get; } = new(12d);

        public virtual IReadOnlyReactiveProperty<bool> CanSelectFontWeight { get; } = new ReactiveProperty<bool>(true);
        public IReadOnlyCollection<FontWeightData> FontWeightDataCollection { get; } = s_fontWeightDataList;
        public ReactiveProperty<FontWeightData> SelectedFontWeightData { get; } = new(GetDefaultFontWeightData());
        public ReadOnlyReactiveProperty<FontWeight> SelectedFontWeight { get; }

        public virtual IReadOnlyReactiveProperty<bool> CanSelectFontFamily { get; } = new ReactiveProperty<bool>(true);
        public ICollection<FontFamily> FontFamilyCollection { get; } = GetFontFamilyCollection();
        public ReactiveProperty<FontFamily> SelectedFontFamily { get; } = new(GetSystemDefaultFont());

        public virtual IReadOnlyReactiveProperty<bool> CanSelectForeground { get; } = new ReactiveProperty<bool>(true);
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

        private static FontWeightData GetDefaultFontWeightData()
        {
            return s_fontWeightDataList.FirstOrDefault(x => x.Name == "Normal");
        }

        private static FontFamily GetSystemDefaultFont()
        {
            var fonts = GetFontFamilyCollection();
            var defaultFont = new FontFamily(System.Drawing.SystemFonts.DefaultFont.FontFamily.Name);
            return fonts.Contains(defaultFont) ? defaultFont : fonts.FirstOrDefault();
        }

        #region IEditor
        public class BaseTextBlockEditorViewModelParameter
        {
            public static readonly string Key = "BaseText";

            [JsonProperty]
            public double FontSize { get; init; } = 12d;
            [JsonProperty]
            public string SelectedFontWeightData { get; init; } = "";
            [JsonProperty]
            public string SelectedFontFamily { get; init; } = GetSystemDefaultFont().Source;
            [JsonProperty]
            public string Foreground { get; init; } = ColorHelper.ToString(Colors.White);
        }

        public override async Task<JObject> SaveAsync(SaveAccessory accessory)
        {
            var jobject = await base.SaveAsync(accessory);
            var param = new BaseTextBlockEditorViewModelParameter()
            {
                FontSize               = FontSize.Value,
                SelectedFontWeightData = SelectedFontWeightData.Value.Name,
                SelectedFontFamily     = SelectedFontFamily.Value.Source,
                Foreground             = ColorHelper.ToString(Foreground.Value)
            };
            jobject[BaseTextBlockEditorViewModelParameter.Key] = JToken.FromObject(param);

            return jobject;
        }

        public override async Task LoadAsync(LoadAccessory accessory, JObject jobject)
        {
            await base.LoadAsync(accessory, jobject);

            if (!jobject.TryGetValue(BaseTextBlockEditorViewModelParameter.Key, out var val))
                return;

            var param = val.ToObject<BaseTextBlockEditorViewModelParameter>();
            if (param is null)
                return;

            FontSize.Value               = param.FontSize;
            SelectedFontWeightData.Value = s_fontWeightDataList.FirstOrDefault(x => x.Name == param.SelectedFontWeightData) ?? s_fontWeightDataList.FirstOrDefault(x => x.Name == "Normal");
            SelectedFontFamily.Value     = FontFamilyCollection.FirstOrDefault(x => x.Source == param.SelectedFontFamily) ?? GetSystemDefaultFont();
            Foreground.Value             = ColorHelper.FromString(param.Foreground);
        }
        #endregion
    }
}
