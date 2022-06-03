using System;
using System.IO;
using System.Threading.Tasks;
using TuringSmartScreenTool.Controllers.Interfaces;
using TuringSmartScreenTool.Entities;

namespace TuringSmartScreenTool.UseCases.Interfaces
{
    public interface IEditCanvasUseCase
    {
        string GetFileExtension();
        Task<EditorFileData> LoadFromDirectoryAsync(DirectoryInfo srcDirectoryInfo, Func<EditorType, IEditor> editorCreateFunction);
        Task<EditorFileData> LoadFromFileAsync(FileInfo loadFileInfo, DirectoryInfo destinationDirectoryInfo, Func<EditorType, IEditor> editorCreateFunction);
        Task SaveEditorAsDirectoryAsync(DirectoryInfo saveDirectoryInfo, EditorFileData editorFileData);
        Task SaveEditorAsFileAsync(FileInfo saveFileInfo, EditorFileData editorFileData);
    }
}
