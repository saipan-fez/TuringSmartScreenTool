using System.Globalization;
using System.Threading.Tasks;
using WeatherLib.Entities;

namespace TuringSmartScreenTool.UseCases.Interfaces
{
    public interface ISearchGeocodeUseCase
    {
        Task<Geocode> SearchAsync(RegionInfo country, string state, string city);
    }
}
