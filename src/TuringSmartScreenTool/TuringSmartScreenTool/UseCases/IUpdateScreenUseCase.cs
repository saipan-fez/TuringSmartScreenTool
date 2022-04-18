using System;
using OpenCvSharp;
using TuringSmartScreenTool.Entities;

namespace TuringSmartScreenTool.UseCases
{
    public interface IUpdateScreenUseCase
    {
        void Start(ScreenDevice screenDevice, Action<Mat<Vec3b>> updateScreenAction);
        void Stop(ScreenDevice screenDevice);
    }
}
