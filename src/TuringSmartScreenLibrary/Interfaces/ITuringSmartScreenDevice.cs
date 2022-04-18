using OpenCvSharp;
using TuringSmartScreenLibrary.Entities;

namespace TuringSmartScreenLibrary.Interfaces
{
    public interface ITuringSmartScreenDevice
    {
        double Brightness { get; }
        Capabilities BrightnessCapabilities { get; }
        bool IsOpen { get; }
        bool IsScreenTurnedOn { get; }
        string Name { get; }
        Rotation Rotate { get; }

        void Dispose();

        void Open(SerialDevice serialDevice, Rotation rotation);
        void Close();

        void RefreshScreen(Mat<Vec3b> mat);
        void ClearScreen();

        Size GetScreenSize();
        void SetBrightness(double value);
        void SetRotation(Rotation rotation);
        void TurnOnScreen();
        void TurnOffScreen();
    }
}
