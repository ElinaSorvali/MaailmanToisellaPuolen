using System;
using Microsoft.Maui.Controls;
using System.Globalization;

namespace MaailmanToisellaPuolen
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            CheckAndRequestLocationPermission();
        }

        public async void CheckAndRequestLocationPermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            if (status == PermissionStatus.Granted)
            {
                await GetCurrentLocation();
            }
            else
            {
                lat_label.Text = "Sijaintia ei voitu hakea: lupa evätty";
            }
        }

        public async Task GetCurrentLocation()
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
                var location = await Geolocation.Default.GetLocationAsync(request);

                if (location != null)
                {
                    // Päivitä sijainnilla
                    lat_label.Text = $"Leveys: {location.Latitude}";
                    lon_label.Text = $"Pituus: {location.Longitude}";

                    // Laske ja näytä antipodi
                    var (antipodeLat, antipodeLon) = CalculateAntipode(location.Latitude, location.Longitude);
                    antipodi_label.Text = $"Antipodi:\nLeveys: {antipodeLat}\nPituus: {antipodeLon}";

                    // Lisää linkki Google Mapsiin
                    var antipodeLink = new Button
                    {
                        Text = "Näytä antipodi Google Mapsissa",
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.Black
                    };
                    antipodeLink.Clicked += async (s, e) =>
                    {
                        var mapsUrl = $"https://www.google.com/maps/search/?api=1&query=" +
                                      $"{antipodeLat.ToString(CultureInfo.InvariantCulture)},{antipodeLon.ToString(CultureInfo.InvariantCulture)}";
                        await Launcher.OpenAsync(new Uri(mapsUrl));
                    };

                    // Lisää nappi sivulle
                    MainLayout.Children.Add(antipodeLink);
                }
                else
                {
                    lat_label.Text = "Sijaintia ei löydy.";
                }
            }
            catch (Exception ex)
            {
                lat_label.Text = "Virhe: " + ex.Message;
            }
        }

        private (double, double) CalculateAntipode(double latitude, double longitude)
        {
            double antipodeLat = -latitude;
            double antipodeLon = (longitude + 180) % 360;

            if (antipodeLon > 180)
            {
                antipodeLon -= 360;
            }

            return (antipodeLat, antipodeLon);
        }
    }
}
