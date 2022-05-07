using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reactive.Bindings;

namespace TuringSmartScreenTool.ViewModels.Editors
{
    public class ImageEditorViewModel : BaseEditorViewModel
    {
        public override ReactiveProperty<string> Name { get; } = new("Image");
        public ReactiveProperty<string> ImageFilePath { get; } = new();

        public ICommand SelectImageCommand { get; }

        public ImageEditorViewModel()
        {
            SelectImageCommand = new RelayCommand(() => SelectImageFilePath(ImageFilePath));
        }

        #region IEditor
        public class ImageEditorViewModelParameter
        {
            public static readonly string Key = "Image";

            [JsonProperty]
            public string ImageFilePath { get; init; } = null;
        }

        public override async Task<JObject> SaveAsync(SaveAccessory accessory)
        {
            var destImageFilePath = accessory.SaveAssetFile(ImageFilePath.Value);

            var jobject = await base.SaveAsync(accessory);
            var param = new ImageEditorViewModelParameter()
            {
                ImageFilePath = destImageFilePath,
            };
            jobject[ImageEditorViewModelParameter.Key] = JToken.FromObject(param);

            return jobject;
        }

        public override async Task LoadAsync(LoadAccessory accessory, JObject jobject)
        {
            await base.LoadAsync(accessory, jobject);

            if (!jobject.TryGetValue(ImageEditorViewModelParameter.Key, out var val))
                return;

            var param = val.ToObject<ImageEditorViewModelParameter>();
            if (param is null)
                return;

            ImageFilePath.Value = accessory.GetFilePath(param.ImageFilePath);
        }
        #endregion
    }
}
