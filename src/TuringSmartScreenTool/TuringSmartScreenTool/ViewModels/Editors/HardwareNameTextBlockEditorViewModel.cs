using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Input;
using ModernWpf.Controls;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TuringSmartScreenTool.Controllers;
using TuringSmartScreenTool.Entities;
using TuringSmartScreenTool.Views;

namespace TuringSmartScreenTool.ViewModels.Editors
{
    public class HardwareNameTextBlockEditorViewModel : BaseTextBlockEditorViewModel
    {
        private readonly ReactiveProperty<string> _hardwareId;
        private readonly IHardwareSelectContentDialog _hardwareSelectContentDialog;
        private readonly ReadOnlyReactiveProperty<IHardwareInfo> _hardware;

        public override ReactiveProperty<string> Name { get; } = new("Hardware Name");
        public override ReadOnlyReactiveProperty<string> Text { get; }

        public ICommand SelectHardwareCommand { get; }

        public HardwareNameTextBlockEditorViewModel(
            IHardwareSelectContentDialog hardwareSelectContentDialog,
            // TODO: usecase
            IHardwareFinder hardwareFinder)
            : this(null, hardwareSelectContentDialog, hardwareFinder)
        {
            _hardwareSelectContentDialog = hardwareSelectContentDialog;
        }

        public HardwareNameTextBlockEditorViewModel(
            string hardwareId,
            IHardwareSelectContentDialog hardwareSelectContentDialog,
            // TODO: usecase
            IHardwareFinder hardwareFinder)
        {
            _hardwareId = new(hardwareId);
            _hardwareSelectContentDialog = hardwareSelectContentDialog;
            _hardware = _hardwareId
                .Select(id => hardwareFinder.Find(id))
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
    }
}
