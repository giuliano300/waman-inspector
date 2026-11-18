using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace Waman_app.Controllers
{
    public class GeocodingController : Controller
    {
        private static readonly string ApiKeyGeocoding = "AIzaSyC1bX005MsZVJqDKc98gN43RfBA0N5YT3Q";
        private static readonly HttpClient GeocodingClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
        };

        [HttpPost]
        public async Task<JsonResult> GeocodeAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                return Json(new { success = false, message = "Address is required" });
            }

            try
            {
                var geocodingUrl = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(address)}&key={ApiKeyGeocoding}";
                var response = await GeocodingClient.GetAsync(geocodingUrl).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var result = JsonConvert.DeserializeObject<GoogleGeocodingResponse>(content);

                    if (result != null && result.status == "OK" && result.results != null && result.results.Length > 0)
                    {
                        var location = result.results[0].geometry.location;
                        return Json(new { success = true, latitude = location.lat, longitude = location.lng });
                    }
                }
            }
            catch (HttpRequestException)
            {
                // Marker synchronization is best-effort and must not break the dashboard.
            }
            catch (TaskCanceledException)
            {
                // Treat provider timeouts as a normal background-sync failure.
            }

            return Json(new { success = false, message = "Geocoding failed" });
        }

        private class GoogleGeocodingResponse
        {
            public string status { get; set; }
            public Result[] results { get; set; }
        }

        private class Result
        {
            public Geometry geometry { get; set; }
        }

        private class Geometry
        {
            public Location location { get; set; }
        }

        private class Location
        {
            public double lat { get; set; }
            public double lng { get; set; }
        }
    }
}
