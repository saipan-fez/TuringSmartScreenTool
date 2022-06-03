using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TuringSmartScreenTool.Controllers.Interfaces;
using TuringSmartScreenTool.Entities;
using TuringSmartScreenTool.UseCases.Interfaces;

namespace TuringSmartScreenTool.UseCases
{
    public class EditCanvasUseCase : IEditCanvasUseCase
    {
        private readonly ILogger<EditCanvasUseCase> _logger;
        private readonly IEditorFileManager _editorFileManager;

        public EditCanvasUseCase(
            ILogger<EditCanvasUseCase> logger,
            IEditorFileManager editorFileManager)
        {
            _logger = logger;
            _editorFileManager = editorFileManager;
        }

        public string GetFileExtension()
        {
            return _editorFileManager.GetFileExtension();
        }

        public async Task<EditorFileData> LoadFromDirectoryAsync(
            DirectoryInfo srcDirectoryInfo,
            Func<EditorType, IEditor> editorCreateFunction)
        {
            return await _editorFileManager.LoadFromDirectoryAsync(srcDirectoryInfo, editorCreateFunction);
        }

        public async Task<EditorFileData> LoadFromFileAsync(
            FileInfo loadFileInfo,
            DirectoryInfo destinationDirectoryInfo,
            Func<EditorType, IEditor> editorCreateFunction)
        {
            return await _editorFileManager.LoadFromFileAsync(loadFileInfo, destinationDirectoryInfo, editorCreateFunction);
        }

        public async Task SaveEditorAsDirectoryAsync(DirectoryInfo saveDirectoryInfo, EditorFileData editorFileData)
        {
            await _editorFileManager.SaveEditorAsDirectoryAsync(saveDirectoryInfo, editorFileData);
        }

        public async Task SaveEditorAsFileAsync(FileInfo saveFileInfo, EditorFileData editorFileData)
        {
            await _editorFileManager.SaveEditorAsFileAsync(saveFileInfo, editorFileData);
        }
    }


}
