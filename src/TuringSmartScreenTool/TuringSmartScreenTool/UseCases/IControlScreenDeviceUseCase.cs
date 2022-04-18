using OpenCvSharp;
using TuringSmartScreenTool.Entities;

namespace TuringSmartScreenTool.UseCases
{
    public interface IControlScreenDeviceUseCase
    {
        void Connect(ScreenDevice screenDevice, OrientationType orientation);
        void Disconnect(ScreenDevice screenDevice);

        Size GetScreenSize(ScreenDevice screenDevice);
        OrientationType GetOrientation(ScreenDevice screenDevice);
        void SetOrientation(ScreenDevice screenDevice, OrientationType orientation);
        (double value, Capabilities capabilities) GetBrightness(ScreenDevice screenDevice);
        void SetBrightness(ScreenDevice screenDevice, double value);
        bool IsScreenTurnedOn(ScreenDevice screenDevice);
        void TurnOnScreen(ScreenDevice screenDevice);
        void TurnOffScreen(ScreenDevice screenDevice);
    }
}
