using System;

namespace TuringSmartScreenTool.Controllers
{
    public interface IValueUpdateManager
    {
        string Register(string name, TimeSpan interval, Action updateAction);
        void Unregister(string id);
    }
}
