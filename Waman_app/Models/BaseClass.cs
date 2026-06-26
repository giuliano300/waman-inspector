using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace Waman_app.Models
{
    public class BaseClass
    {
        public const string WamanAuthTokenSessionKey = "WamanAuthToken";

        public static string staticUrl = "https://inspector.waman.app/";

        //BACKEND HUB
        public static string apiUri = "https://auth.waman.app/api/";
        public static string apiUriLocal = "https://auth.waman.app/api/";



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

        public static async Task<HttpResponseMessage> HttpClientSend(string type, string url, object o = null)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(apiUri);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            AddAuthorizationHeader(client);
            client.Timeout = TimeSpan.FromMinutes(30);

            HttpResponseMessage r = new HttpResponseMessage();
            switch (type.ToUpper())
            {
                case "POST":
                    r = await client.PostAsJsonAsync(url, o);
                    break;
                case "PUT":
                    r = await client.PutAsJsonAsync(url, o);
                    break;
                case "GET":
                    r = await client.GetAsync(url);
                    break;
                default:
                    break;
            }

            return r;

        }

        public static string NoFoto()
        {
            return "/images/noFoto.jpg";
        }

        public static async Task<HttpResponseMessage> HttpClientGet(string url)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(apiUri);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            AddAuthorizationHeader(client);

            var get = await client.GetAsync(url);
            return get;
        }

        private static void AddAuthorizationHeader(HttpClient client)
        {
            var token = System.Web.HttpContext.Current?.Session?[WamanAuthTokenSessionKey] as string;
            if (!string.IsNullOrWhiteSpace(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
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
