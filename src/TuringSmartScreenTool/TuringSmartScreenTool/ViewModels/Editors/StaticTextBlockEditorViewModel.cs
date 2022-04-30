using Reactive.Bindings;

namespace TuringSmartScreenTool.ViewModels.Editors
{
    public class StaticTextBlockEditorViewModel : BaseTextBlockEditorViewModel
    {
        public override ReactiveProperty<string> Text { get; } = new("Text");
    }
}
