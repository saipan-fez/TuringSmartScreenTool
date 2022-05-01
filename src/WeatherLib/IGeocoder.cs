using System.Globalization;
using System.Threading.Tasks;
using WeatherLib.Entities;

namespace WeatherLib
{

    public interface IGeocoder
    {
        Task<Geocode> SearchAsync(RegionInfo country, string state, string city);
    }
}
