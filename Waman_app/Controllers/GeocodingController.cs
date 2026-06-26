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

        [HttpPost]
        public async Task<JsonResult> GeocodeAddress(string address)
        {
            using (var client = new HttpClient())
            {
                var geocodingUrl = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(address)}&key={ApiKeyGeocoding}";
                var response = await client.GetAsync(geocodingUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GoogleGeocodingResponse>(content);

                    if (result.status == "OK" && result.results.Length > 0)
                    {
                        var location = result.results[0].geometry.location;
                        return Json(new { success = true, latitude = location.lat, longitude = location.lng });
                    }
                }

                return Json(new { success = false, message = "Geocoding failed" });
            }
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