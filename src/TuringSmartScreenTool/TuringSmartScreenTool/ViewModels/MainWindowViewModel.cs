using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.Input;
using OpenCvSharp;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TuringSmartScreenTool.Entities;
using TuringSmartScreenTool.Helpers;
using TuringSmartScreenTool.UseCases;

namespace TuringSmartScreenTool.ViewModels
{
    public class MainWindowViewModel : IDisposable
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        private readonly ILogger<MainWindowViewModel> _logger;
        private readonly IFindScreenDeviceUseCase _findScreenDeviceUseCase;
        private readonly IControlScreenDeviceUseCase _controlScreenDeviceUseCase;
        private readonly IUpdateScreenUseCase _updateScreenUseCase;

        public ReactiveProperty<IEnumerable<ScreenDevice>> ScreenDeviceCollection { get; } = new();
        public ReactiveProperty<ScreenDevice> SelectedScreenDevice { get; } = new();

        public ReactiveProperty<OrientationType> Orientation { get; } = new(OrientationType.Portrait);
        public ReactiveProperty<bool> IsDeviceConnecting { get; } = new(false);
        public ReactiveProperty<bool> IsScreenUpdating { get; } = new(false);
        public ReactiveProperty<double> MinBrightness { get; } = new(0);
        public ReactiveProperty<double> MaxBrightness { get; } = new(1);
        public ReactiveProperty<double> StepBrightness { get; } = new(1);
        public ReactiveProperty<double> Brightness { get; } = new(0, ReactivePropertyMode.DistinctUntilChanged);
        public ReactiveProperty<bool> IsScreenTurnedOn { get; } = new(false, ReactivePropertyMode.DistinctUntilChanged);

        public ICommand WindowLoadedCommand { get; }
        public ICommand ConnectDeviceCommand { get; }
        public ICommand DisconnectDeviceCommand { get; }
        public ICommand RefreshScreenDeviceCollectionCommand { get; }
        public ICommand StartScreenUpdateCommand { get; }
        public ICommand StopScreenUpdateComman { get; }

        public MainWindowViewModel(
            ILogger<MainWindowViewModel> logger,
            IFindScreenDeviceUseCase findScreenDeviceUseCase,
            IControlScreenDeviceUseCase controlScreenDeviceUseCase,
            IUpdateScreenUseCase updateScreenUseCase)
        {
            _logger = logger;
            _findScreenDeviceUseCase = findScreenDeviceUseCase;
            _controlScreenDeviceUseCase = controlScreenDeviceUseCase;
            _updateScreenUseCase = updateScreenUseCase;

            WindowLoadedCommand = new RelayCommand(WindowLoaded);
            ConnectDeviceCommand =
                Observable.CombineLatest(
                    SelectedScreenDevice,
                    IsDeviceConnecting,
                    (device, isConnecting) => device is not null && !isConnecting)
                .ToReactiveCommand()
                .WithSubscribe(ConnectDevice)
                .AddTo(_disposables);
            DisconnectDeviceCommand = IsDeviceConnecting
                .ToReactiveCommand()
                .WithSubscribe(DisconnectDevice)
                .AddTo(_disposables);
            RefreshScreenDeviceCollectionCommand = IsDeviceConnecting
                .Select(x => !x)
                .ToReactiveCommand()
                .WithSubscribe(RefreshScreenDeviceCollection)
                .AddTo(_disposables);
            StartScreenUpdateCommand =
                Observable.CombineLatest(
                    IsDeviceConnecting,
                    IsScreenUpdating,
                    (connecting, updating) => connecting && !updating)
                .ToReactiveCommand<Visual>()
                .WithSubscribe(StartToUpdateScreen)
                .AddTo(_disposables);
            StopScreenUpdateComman =
                Observable.CombineLatest(
                    IsDeviceConnecting,
                    IsScreenUpdating,
                    (connecting, updating) => connecting && updating)
                .ToReactiveCommand()
                .WithSubscribe(StopToUpdateScreen)
                .AddTo(_disposables);

            Brightness
                .Subscribe(x => _controlScreenDeviceUseCase.SetBrightness(SelectedScreenDevice.Value, x))
                .AddTo(_disposables);
            IsScreenTurnedOn
                .Subscribe(x =>
                {
                    var d = SelectedScreenDevice.Value;
                    if (x)
                        _controlScreenDeviceUseCase.TurnOnScreen(d);
                    else
                        _controlScreenDeviceUseCase.TurnOffScreen(d);
                })
                .AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private void WindowLoaded()
        {
            RefreshScreenDeviceCollection();
        }

        private void RefreshScreenDeviceCollection()
        {
            var screenDevices = _findScreenDeviceUseCase.Find();
            ScreenDeviceCollection.Value = screenDevices;
            SelectedScreenDevice.Value = screenDevices.FirstOrDefault();
        }

        private void ConnectDevice()
        {
            var d = SelectedScreenDevice.Value;
            if (d is null)
            {
                // TODO: error message
            }

            try
            {
                _controlScreenDeviceUseCase.Connect(d, Orientation.Value);
                var brightness = _controlScreenDeviceUseCase.GetBrightness(d);
                var size = _controlScreenDeviceUseCase.GetScreenSize(d);
                var orientation = _controlScreenDeviceUseCase.GetOrientation(d);
                var isScreenTurnedOn = _controlScreenDeviceUseCase.IsScreenTurnedOn(d);

                MaxBrightness.Value = brightness.capabilities.Max;
                MinBrightness.Value = brightness.capabilities.Min;
                StepBrightness.Value = brightness.capabilities.Step;
                Brightness.Value = brightness.value;
                //CanvasWidth.Value = size.Width;
                //CanvasHeight.Value = size.Height;
                IsScreenTurnedOn.Value = isScreenTurnedOn;
                Orientation.Value = orientation;
                IsDeviceConnecting.Value = true;
            }
            catch
            {
                // TODO: error handling
            }
        }

        private void DisconnectDevice()
        {
            var d = SelectedScreenDevice.Value;
            _controlScreenDeviceUseCase.Disconnect(d);

            IsDeviceConnecting.Value = false;
            IsScreenUpdating.Value = false;
        }

        private void StartToUpdateScreen(Visual visual)
        {
            var d = SelectedScreenDevice.Value;
            var size = _controlScreenDeviceUseCase.GetScreenSize(d);

            _updateScreenUseCase.Start(d, targetBgrMat =>
            {
                // Convert UIElement to byte array(bgra)
                using var bgraMat = new Mat<Vec4b>(size.Height, size.Width);
                VisualHelper.CopyVisualToBgraMat(visual, bgraMat);

                // BGRA8 -> BGR8
                Cv2.CvtColor(bgraMat, targetBgrMat, ColorConversionCodes.BGRA2BGR);
            });

            IsScreenUpdating.Value = true;
        }

        private void StopToUpdateScreen()
        {
            var d = SelectedScreenDevice.Value;
            _updateScreenUseCase.Stop(d);

            IsScreenUpdating.Value = false;
        }
    }
}
