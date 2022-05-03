using System;
using System.Collections.Generic;
using OpenCvSharp;
using TuringSmartScreenTool.Entities;

namespace TuringSmartScreenTool.Controllers.Interfaces
{
    public interface IScreenDeviceManager
    {
        void Dispose();

        IReadOnlyCollection<ScreenDevice> FindDevices();
        void Open(ScreenDevice screenDevice, OrientationType orientation);
        void Close(ScreenDevice screenDevice);

        void SetOrientation(ScreenDevice screenDevice, OrientationType orientation);
        OrientationType GetOrientation(ScreenDevice screenDevice);
        Size GetScreenSize(ScreenDevice screenDevice);
        (double value, Capabilities capabilities) GetBrightness(ScreenDevice screenDevice);
        void SetBrightness(ScreenDevice screenDevice, double value);

        bool IsScreenTurnedOn(ScreenDevice screenDevice);
        void TurnOnScreen(ScreenDevice screenDevice);
        void TurnOffScreen(ScreenDevice screenDevice);

        void StartToUpdateScreen(ScreenDevice screenDevice, Action<Mat<Vec3b>> updateScreenAction);
        void StopToUpdateScreen(ScreenDevice screenDevice);
    }
}
