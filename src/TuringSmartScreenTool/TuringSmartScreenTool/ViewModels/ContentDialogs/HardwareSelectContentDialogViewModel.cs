using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Collections.Generic;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TuringSmartScreenTool.Entities;
using TuringSmartScreenTool.UseCases.Interfaces;

namespace TuringSmartScreenTool.ViewModels.ContentDialogs
{
    public class HardwareSelectContentDialogViewModel : IDisposable
    {
        public enum SelectionMode
        {
            Hardware,
            Sensor,
        }

        private readonly CompositeDisposable _disposables = new();

        public ReactiveProperty<IReadOnlyCollection<IMonitorTarget>> MonitorTargets { get; } = new();
        public ReactiveProperty<IMonitorTarget> SelectedMonitorTarget { get; } = new();
        public ReadOnlyReactiveProperty<bool> IsSelected { get; }

        public ReactiveProperty<SelectionMode> Mode { get; } = new(SelectionMode.Sensor);

        public HardwareSelectContentDialogViewModel(
            IGetMonitorTargetsUseCase getMonitorTargetsUseCase)
        {
            Mode.Subscribe(async mode =>
                {
                    // not include Sensor in MonitorTarges when SelectionMode.Hardware
                    // include Hardware/Sensor in MonitorTarges when SelectionMode.Sensor
                    var targetTypes = mode == SelectionMode.Sensor ?
                        new MonitorTargetType[] { MonitorTargetType.Hardware, MonitorTargetType.Sensor } :
                        new MonitorTargetType[] { MonitorTargetType.Hardware };

                    MonitorTargets.Value = await getMonitorTargetsUseCase.GetMonitorTargetsAsync(targetTypes);
                })
                .AddTo(_disposables);

            IsSelected =
                Observable.CombineLatest(
                    SelectedMonitorTarget,
                    Mode,
                    (s, mode) =>
                    {
                        if (s is null)
                            return false;

                        return mode == SelectionMode.Sensor ?
                            s.Type == MonitorTargetType.Sensor :
                            s.Type == MonitorTargetType.Hardware;
                    })
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}
