using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Input;
using ModernWpf.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TuringSmartScreenTool.Entities;
using TuringSmartScreenTool.UseCases.Interfaces;
using TuringSmartScreenTool.Views.ContentDialogs.Interdfaces;

namespace TuringSmartScreenTool.ViewModels.Editors
{
    public class HardwareNameTextBlockEditorViewModel : BaseTextBlockEditorViewModel
    {
        private readonly ReactiveProperty<string> _hardwareId;
        private readonly IHardwareSelectContentDialog _hardwareSelectContentDialog;
        private readonly ReadOnlyReactiveProperty<IHardwareInfo> _hardware;

        public override EditorType EditorType => EditorType.HardwareName;
        public override ReactiveProperty<string> Name { get; } = new("Hardware Name");
        public override ReadOnlyReactiveProperty<string> Text { get; }

        public ICommand SelectHardwareCommand { get; }

        public HardwareNameTextBlockEditorViewModel(
            IHardwareSelectContentDialog hardwareSelectContentDialog,
            IFindHardwareInfoUseCase findHardwareUseCase)
            : this(null, hardwareSelectContentDialog, findHardwareUseCase)
        {
            _hardwareSelectContentDialog = hardwareSelectContentDialog;
        }

        public HardwareNameTextBlockEditorViewModel(
            string hardwareId,
            IHardwareSelectContentDialog hardwareSelectContentDialog,
            IFindHardwareInfoUseCase findHardwareUseCase)
        {
            _hardwareId = new(hardwareId);
            _hardwareSelectContentDialog = hardwareSelectContentDialog;
            _hardware = _hardwareId
                .Select(id => findHardwareUseCase.Find(id))
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);

            Text = _hardware
                .Select(h => h?.Name)
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);

            SelectHardwareCommand = new RelayCommand(async () =>
            {
                var result = await _hardwareSelectContentDialog.ShowAsync(HardwareSelectType.Hardware);
                if (result == ContentDialogResult.Primary)
                {
                    _hardwareId.Value = _hardwareSelectContentDialog.SelectedId;
                }
            });
        }

        #region IEditor
        public class HardwareNameTextBlockEditorViewModelParameter
        {
            public static readonly string Key = "HardwareNameText";

            [JsonProperty]
            public string HardwareId { get; init; } = null;
        }

        public override async Task<JObject> SaveAsync(SaveAccessory accessory)
        {
            var jobject = await base.SaveAsync(accessory);
            var param = new HardwareNameTextBlockEditorViewModelParameter()
            {
                HardwareId = _hardwareId.Value
            };
            jobject[HardwareNameTextBlockEditorViewModelParameter.Key] = JToken.FromObject(param);

            return jobject;
        }

        public override async Task LoadAsync(LoadAccessory accessory, JObject jobject)
        {
            await base.LoadAsync(accessory, jobject);

            if (!jobject.TryGetValue(HardwareNameTextBlockEditorViewModelParameter.Key, out var val))
                return;

            var param = val.ToObject<HardwareNameTextBlockEditorViewModelParameter>();
            if (param is null)
                return;

            _hardwareId.Value = param.HardwareId;
        }
        #endregion
    }
}
