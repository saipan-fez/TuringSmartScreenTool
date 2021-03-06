using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using WeatherLib;
using TuringSmartScreenTool.UseCases.Interfaces;

namespace TuringSmartScreenTool.ViewModels.ContentDialogs
{
    public class LocationSelectContentDialogViewModel : IDisposable
    {
        private static readonly RegionInfo[] s_countryCollection = GetCountryCollection();

        private readonly CompositeDisposable _disposables = new();

        public RegionInfo[] CountryCollection => s_countryCollection;
        public ReactiveProperty<RegionInfo> SelectedRegionInfo { get; } = new();
        public ReactiveProperty<string> InputState { get; } = new();
        public ReactiveProperty<string> InputCity { get; } = new();
        public ReactiveProperty<bool> IsConvertFailed { get; } = new(false);

        // TODO: validate
        public ReactiveProperty<double?> Latitude { get; } = new((double?)null);
        public ReactiveProperty<double?> Longitude { get; } = new((double?)null);

        public ReadOnlyReactiveProperty<bool> IsInputed { get; }

        public ICommand ConvertAddressToLocationCommand { get; }

        public LocationSelectContentDialogViewModel(
            ISearchGeocodeUseCase searchGeocodeUseCase)
        {
            IsInputed =
                Observable.CombineLatest(
                    Latitude,
                    Longitude,
                    (lat, log) => -90 <= lat && lat <= 90 && -180 <= log && log <= 180)
                .ToReadOnlyReactiveProperty();

            ConvertAddressToLocationCommand = new AsyncReactiveCommand()
                .WithSubscribe(async () =>
                {
                    try
                    {
                        IsConvertFailed.Value = false;

                        var geocode = await searchGeocodeUseCase.SearchAsync(
                            SelectedRegionInfo.Value,
                            InputState.Value,
                            InputCity.Value);

                        Latitude.Value = geocode?.Latitude;
                        Longitude.Value = geocode?.Longitude;

                        IsConvertFailed.Value = geocode == null;
                    }
                    catch
                    {
                        IsConvertFailed.Value = true;
                    }
                });
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private static RegionInfo[] GetCountryCollection()
        {
            return CultureInfo
                .GetCultures(CultureTypes.SpecificCultures)
                .Select(c => new RegionInfo(c.LCID))
                .Distinct()
                .OrderBy(x => x.EnglishName)
                .ToArray();
        }
    }
}
