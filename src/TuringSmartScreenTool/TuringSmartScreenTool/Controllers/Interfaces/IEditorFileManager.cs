using System;
using System.IO;
using System.Threading.Tasks;
using TuringSmartScreenTool.Entities;

namespace TuringSmartScreenTool.Controllers.Interfaces
{
    public interface IEditorFileManager
    {
        string GetFileExtension();
        Task<EditorFileData> LoadFromFileAsync(FileInfo loadFileInfo, DirectoryInfo destinationDirectoryInfo, Func<EditorType, IEditor> editorCreateFunction);
        Task SaveEditorAsFileAsync(FileInfo saveFileInfo, EditorFileData editorFileData);
    }
}
