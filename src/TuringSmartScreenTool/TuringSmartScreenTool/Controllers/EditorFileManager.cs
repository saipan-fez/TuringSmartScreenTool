using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using TuringSmartScreenTool.Controllers.Interfaces;
using TuringSmartScreenTool.Entities;
using TuringSmartScreenTool.Helpers;

namespace TuringSmartScreenTool.Controllers
{
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

        public async Task SaveEditorAsDirectoryAsync(DirectoryInfo saveDirectoryInfo, EditorFileData editorFileData)
        {
            DirectoryInfo tempDirectory = null;

            try
            {
                await Task.Run(async () =>
                {
                    tempDirectory = await SaveEditorAsTempDirectoryAsync(editorFileData);
                    tempDirectory.CopyRecursive(saveDirectoryInfo, true);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "failed to save to dir. saveDir:{saveDirectoryInfo}", saveDirectoryInfo.FullName);
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

        private async Task<DirectoryInfo> SaveEditorAsTempDirectoryAsync(EditorFileData editorFileData)
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
                });
            }
            catch (Exception ex)
            {
                try
                {
                    _logger.LogError(ex, "failed to save the file. saveTo:{tempDirectory}", tempDirectory?.FullName);
                    if (tempDirectory is not null)
                        tempDirectory.DeleteRecursive();
                }
                catch (Exception ex2)
                {
                    _logger.LogError(ex2, "failed to delete temp directory.");
                }
                throw;
            }

            return tempDirectory;
        }

        public async Task SaveEditorAsFileAsync(FileInfo saveFileInfo, EditorFileData editorFileData)
        {
            DirectoryInfo tempDirectory = null;

            try
            {
                await Task.Run(async () =>
                {
                    tempDirectory = await SaveEditorAsTempDirectoryAsync(editorFileData);
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

        public async Task<EditorFileData> LoadFromDirectoryAsync(DirectoryInfo srcDirectoryInfo, Func<EditorType, IEditor> editorCreateFunction)
        {
            try
            {
                string json;
                var jsonFilePath = Path.Combine(srcDirectoryInfo.FullName, s_canvasJsonFileName);
                using (var sr = new StreamReader(jsonFilePath))
                {
                    json = await sr.ReadToEndAsync();
                }

                var parameter = JsonConvert.DeserializeObject<CanvasEditorData>(json);
                var loadAccessory = new LoadAccessory(srcDirectoryInfo);
                var editors = new List<(EditorType editorTyp, IEditor editor)>();
                foreach (var (editorType, jobject) in parameter.Editors)
                {
                    var editor = editorCreateFunction(editorType);
                    await editor.LoadAsync(loadAccessory, jobject);
                    editors.Add((editorType, editor));
                }

                return new()
                {
                    Editors = editors,
                    CanvasBackgroundType = parameter.CanvasBackgroundType,
                    CanvasBackgroundColor = parameter.CanvasBackgroundColor,
                    CanvasBackgroundImagePath = loadAccessory.GetFilePath(parameter.CanvasBackgroundImagePath),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "failed to load the directory. srcDir:{directoryPath}", srcDirectoryInfo.FullName);
                throw;
            }
        }

        public async Task<EditorFileData> LoadFromFileAsync(FileInfo loadFileInfo, DirectoryInfo destinationDirectoryInfo, Func<EditorType, IEditor> editorCreateFunction)
        {
            if (!loadFileInfo.Exists)
                throw new FileNotFoundException("file not found.", loadFileInfo.FullName);

            try
            {
                ZipFile.ExtractToDirectory(loadFileInfo.FullName, destinationDirectoryInfo.FullName);
                return await LoadFromDirectoryAsync(destinationDirectoryInfo, editorCreateFunction);
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
