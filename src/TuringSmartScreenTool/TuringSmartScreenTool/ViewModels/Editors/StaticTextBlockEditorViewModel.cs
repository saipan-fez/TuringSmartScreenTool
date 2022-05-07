using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reactive.Bindings;

namespace TuringSmartScreenTool.ViewModels.Editors
{
    public class StaticTextBlockEditorViewModel : BaseTextBlockEditorViewModel
    {
        public override ReactiveProperty<string> Text { get; } = new("Text");

        #region IEditor
        public class StaticTextBlockEditorViewModelParameter
        {
            public static readonly string Key = "StaticText";

            [JsonProperty]
            public string Text { get; init; } = "Text";
        }

        public override async Task<JObject> SaveAsync(SaveAccessory accessory)
        {
            var jobject = await base.SaveAsync(accessory);
            var param = new StaticTextBlockEditorViewModelParameter()
            {
                Text = Text.Value,
            };
            jobject[StaticTextBlockEditorViewModelParameter.Key] = JToken.FromObject(param);

            return jobject;
        }

        public override async Task LoadAsync(LoadAccessory accessory, JObject jobject)
        {
            await base.LoadAsync(accessory, jobject);

            if (!jobject.TryGetValue(StaticTextBlockEditorViewModelParameter.Key, out var val))
                return;

            var param = val.ToObject<StaticTextBlockEditorViewModelParameter>();
            if (param is null)
                return;

            Text.Value = param.Text;
        }
        #endregion
    }
}
