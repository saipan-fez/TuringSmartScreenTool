using System;
using System.Threading;
using System.Threading.Tasks;

namespace TuringSmartScreenTool.Controllers.Interfaces
{
    public interface IValueUpdateManager
    {
        string Register(string name, TimeSpan interval, Action updateAction);
        string Register(string name, TimeSpan interval, Func<CancellationToken, Task> updateAsyncFunction);
        void Unregister(string id);
    }
}
