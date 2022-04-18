using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Input;
using Reactive.Bindings;

namespace TuringSmartScreenTool.ViewModels
{
    public class ImageEditorViewModel : CommonEditorViewModel
    {
        public override ReactiveProperty<string> Name { get; } = new("Image");
        public ReactiveProperty<string> ImageFilePath { get; } = new();

        public ICommand SelectImageCommand { get; }

        public ImageEditorViewModel()
        {
            SelectImageCommand = new RelayCommand(() => SelectImageFilePath(ImageFilePath));
        }
    }
}
