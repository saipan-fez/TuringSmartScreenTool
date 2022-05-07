using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TuringSmartScreenTool.Entities;

namespace TuringSmartScreenTool.Controllers.Interfaces
{
    public interface IEditor
    {
        Task<JObject> SaveAsync(SaveAccessory accessory);
        Task LoadAsync(LoadAccessory accessory, JObject jobject);
    }
}
