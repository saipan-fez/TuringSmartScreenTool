using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TuringSmartScreenTool.Controllers.Interfaces;
using TuringSmartScreenTool.Entities;
using TuringSmartScreenTool.Helpers;
using TuringSmartScreenTool.ViewModels.Editors;
using TuringSmartScreenTool.Views;

namespace TuringSmartScreenTool.ViewModels
{
    public enum EditorType
    {
        Text,
        Image,
        HardwareName,
        HardwareValueText,
        HardwareValueIndicator,
        DateTime,
        Weather,
    }

    public class CanvasEditorWindowViewModel : IDisposable
    {
        private static readonly IEnumerable<EditorType> s_editorTypeCollection = Enum.GetValues(typeof(EditorType)).Cast<EditorType>();

        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        private readonly ILogger<CanvasEditorWindowViewModel> _logger;
        private readonly IHardwareSelectContentDialog _hardwareSelectContentDialog;
        private readonly ILocationSelectContentDialog _locationSelectContentDialog;
        private readonly IWeatherIconPreviewContentDialog _weatherIconPreviewContentDialog;
        private readonly IHardwareFinder _hardwareFinder;
        private readonly ISensorFinder _sensorFinder;
        private readonly ITimeManager _timeManager;
        private readonly IWeatherManager _weatherManager;
        private readonly IEditorFileManager _editorFileManager;

        public ObservableCollection<BaseEditorViewModel> EditorViewModels { get; } = new();
        public ReactiveProperty<int> SelectedEditorViewModelIndex { get; } = new(-1);
        public ReadOnlyReactiveProperty<BaseEditorViewModel> SelectedEditorViewModel { get; }
        public ReadOnlyReactiveProperty<bool> IsEditorViewModelSelected { get; }

        public IEnumerable<EditorType> EditorCollection { get; } = s_editorTypeCollection;
        public ReactiveProperty<EditorType> SelectedEditor { get; } = new(s_editorTypeCollection.FirstOrDefault());

        public ReactiveProperty<int> CanvasWidth { get; } = new(0);
        public ReactiveProperty<int> CanvasHeight { get; } = new(0);

        public ReactiveProperty<CanvasBackgroundType> InputCanvasBackgroundType { get; } = new(CanvasBackgroundType.SolidColor);
        public ReactiveProperty<Color> InputCanvasBackgroundColor { get; } = new(Colors.Black);
        public ReactiveProperty<string> InputCanvasBackgroundImagePath { get; } = new();
        public ReadOnlyReactiveProperty<object> CanvasBackground { get; }

        public ICommand SelectBackgroundColorCommand { get; }
        public ICommand SelectBackgroundImageCommand { get; }

        public ICommand AddEditorCommand { get; }
        public ICommand MoveUpEditorCommand { get; }
        public ICommand MoveDownEditorCommand { get; }
        public ICommand DeleteSelectedEditorCommand { get; }
        public ICommand SaveAsFileCommand { get; }
        public ICommand LoadFromFileCommand { get; }

        public CanvasEditorWindowViewModel(
            ILogger<CanvasEditorWindowViewModel> logger,
            IHardwareSelectContentDialog hardwareSelectContentDialog,
            ILocationSelectContentDialog locationSelectContentDialog,
            IWeatherIconPreviewContentDialog weatherIconPreviewContentDialog,
            // TODO: usecase
            IHardwareFinder hardwareFinder,
            ISensorFinder sensorFinder,
            ITimeManager timeManager,
            IWeatherManager weatherManager,
            IEditorFileManager editorFileManager)
        {
            _logger                          = logger;
            _hardwareSelectContentDialog     = hardwareSelectContentDialog;
            _locationSelectContentDialog     = locationSelectContentDialog;
            _weatherIconPreviewContentDialog = weatherIconPreviewContentDialog;
            _hardwareFinder                  = hardwareFinder;
            _sensorFinder                    = sensorFinder;
            _timeManager                     = timeManager;
            _weatherManager                  = weatherManager;
            _editorFileManager               = editorFileManager;

            SelectedEditorViewModel = SelectedEditorViewModelIndex
                .Select(idx => EditorViewModels.ElementAtOrDefault(idx))
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);
            IsEditorViewModelSelected = SelectedEditorViewModelIndex
                .Select(idx => idx == -1)
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);
            CanvasBackground =
                Observable.CombineLatest(
                    InputCanvasBackgroundType,
                    InputCanvasBackgroundColor,
                    InputCanvasBackgroundImagePath,
                    (type, color, path) => (object)(type switch
                    {
                        CanvasBackgroundType.SolidColor => color,
                        CanvasBackgroundType.Image => path,
                        _ => throw new InvalidOperationException(),
                    }))
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);
            SelectBackgroundColorCommand = InputCanvasBackgroundType
                .Select(x => x == CanvasBackgroundType.SolidColor)
                .ToReactiveCommand()
                .WithSubscribe(_ => SelectColor(InputCanvasBackgroundColor))
                .AddTo(_disposables);
            SelectBackgroundImageCommand = InputCanvasBackgroundType
                .Select(x => x == CanvasBackgroundType.Image)
                .ToReactiveCommand()
                .WithSubscribe(_ => SelectImageFilePath(InputCanvasBackgroundImagePath))
                .AddTo(_disposables);
            AddEditorCommand = new RelayCommand(() => AddEditor());
            MoveUpEditorCommand = SelectedEditorViewModelIndex
                .Select(idx => idx != -1 && idx > 0)
                .ToReactiveCommand()
                .WithSubscribe(_ => MoveUpSelectedEditor())
                .AddTo(_disposables);
            MoveDownEditorCommand = SelectedEditorViewModelIndex
                .Select(idx => idx != -1 && idx < EditorViewModels.Count - 1)
                .ToReactiveCommand()
                .WithSubscribe(_ => MoveDownSelectedEditor())
                .AddTo(_disposables);
            DeleteSelectedEditorCommand = SelectedEditorViewModel
                .Select(x => x != null)
                .ToReactiveCommand()
                .WithSubscribe(_ => DeleteSelectedEditor())
                .AddTo(_disposables);
            SaveAsFileCommand = new AsyncReactiveCommand()
                .WithSubscribe(x => SaveAsFileEditor())
                .AddTo(_disposables);
            LoadFromFileCommand = new AsyncReactiveCommand()
                .WithSubscribe(x => LoadFromFile())
                .AddTo(_disposables);

            // TODO: delete
            CanvasWidth.Value = 320;
            CanvasHeight.Value = 480;
            SelectedEditorViewModelIndex.Value = EditorViewModels.Count > 0 ? 0 : -1;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private void AddEditor()
        {
            var editorViewModel = CreateEditorViewModel(SelectedEditor.Value);
            EditorViewModels.Add(editorViewModel);
            SelectedEditorViewModelIndex.Value = EditorViewModels.Count - 1;
        }

        private BaseEditorViewModel CreateEditorViewModel(EditorType type)
        {
            return type switch
            {
                EditorType.Text                   => new StaticTextBlockEditorViewModel(),
                EditorType.Image                  => new ImageEditorViewModel(),
                EditorType.HardwareName           => new HardwareNameTextBlockEditorViewModel(_hardwareSelectContentDialog, _hardwareFinder),
                EditorType.HardwareValueText      => new HardwareSensorTextBlockEditorViewModel(_hardwareSelectContentDialog, _sensorFinder),
                EditorType.HardwareValueIndicator => new HardwareSensorIndicatorEditorViewModel(_hardwareSelectContentDialog, _sensorFinder),
                EditorType.DateTime               => new DateTimeTextEditorViewModel(_timeManager),
                EditorType.Weather                => new WeatherTextEditorViewModel(_weatherManager, _locationSelectContentDialog, _weatherIconPreviewContentDialog),
                _                                 => throw new InvalidOperationException(),
            };
        }

        private void MoveUpSelectedEditor()
        {
            var index = SelectedEditorViewModelIndex.Value;

            // not found
            if (index == -1)
                return;

            if (index > 0)
            {
                var item = EditorViewModels[index];
                EditorViewModels.RemoveAt(index);
                EditorViewModels.Insert(index - 1, item);
                SelectedEditorViewModelIndex.Value = index - 1;
            }
        }

        private void MoveDownSelectedEditor()
        {
            var index = SelectedEditorViewModelIndex.Value;

            // not found
            if (index == -1)
                return;

            if (index < EditorViewModels.Count - 1)
            {
                var item = EditorViewModels[index];
                EditorViewModels.RemoveAt(index);
                EditorViewModels.Insert(index + 1, item);
                SelectedEditorViewModelIndex.Value = index + 1;
            }
        }

        private void DeleteSelectedEditor()
        {
            var index = SelectedEditorViewModelIndex.Value;

            // not found
            if (index == -1)
                return;

            if (index <= EditorViewModels.Count - 1)
            {
                EditorViewModels.RemoveAt(index);

                if (EditorViewModels.Count > 0)
                    // select previous element
                    SelectedEditorViewModelIndex.Value = index == 0 ? 0 : index - 1;
                else
                    SelectedEditorViewModelIndex.Value = -1;
            }
        }

        private static void SelectColor(ReactiveProperty<Color> target)
        {
            var c = target.Value;
            var colorDialog = new ColorDialog()
            {
                Color = System.Drawing.Color.FromArgb(c.R, c.G, c.B),
                FullOpen = true,
                AnyColor = false
            };
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                var newColor = colorDialog.Color;
                target.Value = Color.FromRgb(newColor.R, newColor.G, newColor.B);
            }
        }

        private static void SelectImageFilePath(ReactiveProperty<string> target)
        {
            var fileDialog = new OpenFileDialog()
            {
                Filter = "Image|*.png;*.jpg;*.jpeg;*.bmp;*.gif" + "|All Files|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                Multiselect = false
            };
            if (fileDialog.ShowDialog() == DialogResult.OK &&
                File.Exists(fileDialog.FileName))
            {
                target.Value = fileDialog.FileName;
            }
        }

        private static EditorType ConvertToEditorType(BaseEditorViewModel vm)
        {
            return vm switch
            {
                StaticTextBlockEditorViewModel         => EditorType.Text,
                ImageEditorViewModel                   => EditorType.Image,
                HardwareNameTextBlockEditorViewModel   => EditorType.HardwareName,
                HardwareSensorTextBlockEditorViewModel => EditorType.HardwareValueText,
                HardwareSensorIndicatorEditorViewModel => EditorType.HardwareValueIndicator,
                DateTimeTextEditorViewModel            => EditorType.DateTime,
                WeatherTextEditorViewModel             => EditorType.Weather,
                _                                      => throw new InvalidOperationException(),
            };
        }

        private async Task LoadFromFile()
        {
            var fileDialog = new OpenFileDialog()
            {
                Filter           = "TSS Files(*.tss)|*.tss" + "|All Files(*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Multiselect      = false
            };

            if (fileDialog.ShowDialog() != DialogResult.OK)
                return;

            var filePath = new FileInfo(fileDialog.FileName);
            var tempDirectory = DirectoryInfoHelper.GetTempDirectory();
            var editorFileData = await _editorFileManager.LoadFromFileAsync(filePath, tempDirectory, CreateEditorViewModel);
            EditorViewModels.Clear();
            foreach (var (_, editor) in editorFileData.Editors)
            {
                if (editor is BaseEditorViewModel vm)
                {
                    EditorViewModels.Add(vm);
                }
            }
            InputCanvasBackgroundType.Value      = editorFileData.CanvasBackgroundType;
            InputCanvasBackgroundColor.Value     = ColorHelper.FromString(editorFileData.CanvasBackgroundColor);
            InputCanvasBackgroundImagePath.Value = editorFileData.CanvasBackgroundImagePath;
        }

        private async Task SaveAsFileEditor()
        {
            var editorFileManager = new EditorFileManager(null);
            var saveFileDialog = new SaveFileDialog()
            {
                DefaultExt       = editorFileManager.GetFileExtension(),
                FileName         = DateTime.Now.ToString("yyyyMMddhhmm"),
                RestoreDirectory = true,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Filter           = "TSS Files(*.tss)|*.tss" + "|All Files(*.*)|*.*",
                OverwritePrompt  = true
            };

            if (saveFileDialog.ShowDialog() != DialogResult.OK)
                return;

            var filePath = new FileInfo(saveFileDialog.FileName);

            var editors = EditorViewModels
                .Select(x => (ConvertToEditorType(x), (IEditor)x))
                .ToList();
            var editorFileData = new EditorFileData()
            {
                Editors                   = editors,
                CanvasBackgroundType      = InputCanvasBackgroundType.Value,
                CanvasBackgroundColor     = ColorHelper.ToString(InputCanvasBackgroundColor.Value),
                CanvasBackgroundImagePath = InputCanvasBackgroundImagePath.Value
            };
            await _editorFileManager.SaveEditorAsFileAsync(filePath, editorFileData);
        }
    }

    public class EditorFileData
    {
        public IReadOnlyList<(EditorType editorType, IEditor editor)> Editors { get; init; }
        public CanvasBackgroundType CanvasBackgroundType { get; init; }
        public string CanvasBackgroundColor { get; init; }
        public string CanvasBackgroundImagePath { get; init; }
    }

    public interface IEditorFileManager
    {
        string GetFileExtension();
        Task<EditorFileData> LoadFromFileAsync(FileInfo loadFileInfo, DirectoryInfo destinationDirectoryInfo, Func<EditorType, IEditor> editorCreateFunction);
        Task SaveEditorAsFileAsync(FileInfo saveFileInfo, EditorFileData editorFileData);
    }

    public class EditorFileManager : IEditorFileManager
    {
        private class CanvasEditorData
        {
            [JsonProperty]
            public string Version { get; init; }
            [JsonProperty]
            public List<(EditorType editorType, JObject jobject)> Editors { get; init; }
            [JsonProperty]
            [JsonConverter(typeof(StringEnumConverter))]
            public CanvasBackgroundType CanvasBackgroundType { get; init; } = CanvasBackgroundType.SolidColor;
            [JsonProperty]
            public string CanvasBackgroundColor { get; init; } = ColorHelper.ToString(Colors.Black);
            [JsonProperty]
            public string CanvasBackgroundImagePath { get; init; } = null;
        }

        private static readonly string s_canvasJsonFileName = "canvas.json";
        private static readonly string s_fileExtension      = ".tss";
        private static readonly string s_assetsDirName      = "assets";

        private readonly ILogger<EditorFileManager> _logger;

        public EditorFileManager(
            ILogger<EditorFileManager> logger)
        {
            _logger = logger;
        }

        public string GetFileExtension()
        {
            return s_fileExtension;
        }

        public async Task SaveEditorAsFileAsync(FileInfo saveFileInfo, EditorFileData editorFileData)
        {
            DirectoryInfo tempDirectory = null;

            try
            {
                await Task.Run(async () =>
                {
                    tempDirectory = DirectoryInfoHelper.GetTempDirectory();

                    var assetsDirectory = tempDirectory.CreateSubdirectory(s_assetsDirName);
                    var saveAccessory   = new SaveAccessory(assetsDirectory);
                    var editorJsons     = new List<(EditorType, JObject)>();
                    foreach (var (type, editor) in editorFileData.Editors)
                    {
                        var j = await editor.SaveAsync(saveAccessory);
                        editorJsons.Add((type, j));
                    }

                    var backgroundImageFilePath = saveAccessory.SaveAssetFile(editorFileData.CanvasBackgroundImagePath);
                    var param = new CanvasEditorData()
                    {
                        Version                   = "1.0",
                        Editors                   = editorJsons,
                        CanvasBackgroundType      = editorFileData.CanvasBackgroundType,
                        CanvasBackgroundColor     = editorFileData.CanvasBackgroundColor,
                        CanvasBackgroundImagePath = backgroundImageFilePath
                    };
                    var json = JsonConvert.SerializeObject(param, Formatting.Indented);
                    var jsonFilePath = Path.Combine(tempDirectory.FullName, s_canvasJsonFileName);
                    using (var sw = new StreamWriter(jsonFilePath))
                    {
                        sw.Write(json);
                    }

                    ZipFile.CreateFromDirectory(tempDirectory.FullName, saveFileInfo.FullName);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "failed to save the file. saveTo:{filePath}", saveFileInfo.FullName);
                throw;
            }
            finally
            {
                try
                {
                    if (tempDirectory is not null)
                        tempDirectory.DeleteRecursive();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "failed to delete temp directory.");
                }
            }
        }

        public async Task<EditorFileData> LoadFromFileAsync(FileInfo loadFileInfo, DirectoryInfo destinationDirectoryInfo, Func<EditorType, IEditor> editorCreateFunction)
        {
            if (!loadFileInfo.Exists)
                throw new FileNotFoundException("file not found.", loadFileInfo.FullName);

            try
            {
                ZipFile.ExtractToDirectory(loadFileInfo.FullName, destinationDirectoryInfo.FullName);

                string json;
                var jsonFilePath = Path.Combine(destinationDirectoryInfo.FullName, s_canvasJsonFileName);
                using (var sr = new StreamReader(jsonFilePath))
                {
                    json = await sr.ReadToEndAsync();
                }

                var parameter     = JsonConvert.DeserializeObject<CanvasEditorData>(json);
                var loadAccessory = new LoadAccessory(destinationDirectoryInfo);
                var editors       = new List<(EditorType editorTyp, IEditor editor)>();
                foreach (var (editorType, jobject) in parameter.Editors)
                {
                    var editor = editorCreateFunction(editorType);
                    await editor.LoadAsync(loadAccessory, jobject);
                    editors.Add((editorType, editor));
                }

                return new()
                {
                    Editors                   = editors,
                    CanvasBackgroundType      = parameter.CanvasBackgroundType,
                    CanvasBackgroundColor     = parameter.CanvasBackgroundColor,
                    CanvasBackgroundImagePath = loadAccessory.GetFilePath(parameter.CanvasBackgroundImagePath),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "failed to load the file. loadFrom:{filePath} destTo:{directoryPath}", loadFileInfo.FullName, destinationDirectoryInfo.FullName);
                try
                {
                    if (destinationDirectoryInfo is not null)
                        destinationDirectoryInfo.DeleteRecursive();
                }
                catch (Exception ex2)
                {
                    _logger.LogError(ex2, "failed to delete directory.");
                }

                throw;
            }
        }
    }
}
