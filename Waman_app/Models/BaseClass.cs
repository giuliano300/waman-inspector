using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace Waman_app.Models
{
    public class BaseClass
    {
        public const string WamanAuthTokenSessionKey = "WamanAuthToken";

        public static string staticUrl = "https://inspector.waman.app/";

        //BACKEND HUB
        public static string apiUri = "https://auth.waman.app/api/";
        public static string apiUriLocal = "https://auth.waman.app/api/";

        private static readonly HttpClient SharedHttpClient = CreateHttpClient();



        //EMAIL
        public static string mittente = "";
        public static string destinatarioAdmin = "";

        //HASHING
        public static string secret = "nuh89d8dknjqkwndjkdn789221!!!";


        public static string ComputeHmac(string data, string secret)
        {
            var key = Encoding.UTF8.GetBytes(secret);

            using (var hmac = new HMACSHA256(key))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));

                return BitConverter
                    .ToString(hash)
                    .Replace("-", "")
                    .ToLowerInvariant();
            }
        }


        public static string GetCSV(string url)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

            StreamReader sr = new StreamReader(resp.GetResponseStream());
            string results = sr.ReadToEnd();
            sr.Close();

            return results;
        }

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(apiUri);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.Timeout = TimeSpan.FromMinutes(30);
            return client;
        }

        public static async Task<HttpResponseMessage> HttpClientSend(string type, string url, object o = null)
        {
            var method = new HttpMethod(type.ToUpperInvariant());
            using (var request = new HttpRequestMessage(method, url))
            {
                AddAuthorizationHeader(request);

                if (method == HttpMethod.Post || method == HttpMethod.Put)
                {
                    var json = JsonConvert.SerializeObject(o);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                return await SharedHttpClient.SendAsync(request).ConfigureAwait(false);
            }
        }

        public static string NoFoto()
        {
            return "/images/noFoto.jpg";
        }

        public static async Task<HttpResponseMessage> HttpClientGet(string url)
        {
            return await HttpClientSend("GET", url).ConfigureAwait(false);
        }

        private static void AddAuthorizationHeader(HttpRequestMessage request)
        {
            var context = System.Web.HttpContext.Current;
            var token = context?.Session?[WamanAuthTokenSessionKey] as string;

            if (string.IsNullOrWhiteSpace(token))
            {
                var inspectorCookie = context?.Request?.Cookies?["Inspectors"]?.Value;
                if (!string.IsNullOrWhiteSpace(inspectorCookie))
                {
                    var inspector = JsonConvert.DeserializeObject<Inspectors>(inspectorCookie);
                    token = inspector?.authToken;

                    if (!string.IsNullOrWhiteSpace(token) && context?.Session != null)
                        context.Session[WamanAuthTokenSessionKey] = token;
                }
            }

            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public static string rewriteUrl(string Stringa)
        {
            Stringa = Stringa.Replace("'", "-");
            Stringa = Stringa.Replace(".", "-");
            Stringa = Stringa.Replace(",", "-");
            Stringa = Stringa.Replace("è", "e");
            Stringa = Stringa.Replace("à", "a");
            Stringa = Stringa.Replace("ò", "o");
            Stringa = Stringa.Replace(" ", "-");
            Stringa = Stringa.Replace("/", "-");
            Stringa = Stringa.Replace("&", "-");
            Stringa = Stringa.Replace(":", "-");
            Stringa = Stringa.Replace(";", "-");
            return Stringa;
        }

        public static string leftString(string Stringa, int index)
        {
            return Stringa.Substring(0, Math.Min(index, Stringa.Length));
        }

        public static string GenerateToken()
        {
            Guid g = Guid.NewGuid();
            string GuidString = Convert.ToBase64String(g.ToByteArray());
            GuidString = GuidString.Replace("=", "");
            GuidString = GuidString.Replace("+", "");
            return GuidString;
        }

        public static string GetMonthName(int month)
        {
            switch (month)
            {
                case 1:
                    return "Gennaio";
                case 2:
                    return "Febbraio";
                case 3:
                    return "Marzo";
                case 4:
                    return "Aprile";
                case 5:
                    return "Maggio";
                case 6:
                    return "Giugno";
                case 7:
                    return "Luglio";
                case 8:
                    return "Agosto";
                case 9:
                    return "Settembre";
                case 10:
                    return "Ottobre";
                case 11:
                    return "Novembre";
                case 12:
                    return "Dicembre";
            }
            return null;
        }
        public static string GetCSVFromRemote(string url)
        {
            FileWebRequest req = (FileWebRequest)FileWebRequest.Create(url);
            FileWebResponse resp = (FileWebResponse)req.GetResponse();

            StreamReader sr = new StreamReader(resp.GetResponseStream());
            string results = sr.ReadToEnd();
            sr.Close();

            return results;
        }

        public static async Task<List<string>> ReadCsv(string path)
        {
            var r = new List<string>();
            string fileList = BaseClass.GetCSV(path);
            string[] tempStr;

            tempStr = fileList.Split((char)10);

            foreach (string row in tempStr)
                if (row != null && row != "")
                    r.Add(row.ToString().Replace("\r", ""));

            return r;

        }
    }
}
