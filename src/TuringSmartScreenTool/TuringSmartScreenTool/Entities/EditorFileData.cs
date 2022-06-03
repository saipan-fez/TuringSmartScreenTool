using System.Collections.Generic;
using TuringSmartScreenTool.Controllers.Interfaces;

namespace TuringSmartScreenTool.Entities
{
    public class EditorFileData
    {
        public CanvasSize CanvasSize { get; init; }
        public IReadOnlyList<(EditorType editorType, IEditor editor)> Editors { get; init; }
        public CanvasBackgroundType CanvasBackgroundType { get; init; }
        public string CanvasBackgroundColor { get; init; }
        public string CanvasBackgroundImagePath { get; init; }
    }
}
