using System;

namespace Waman_app.Models
{
    public class Locations
    {
        public int id { get; set; }
        public int entityId { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public string entityRapresentative { get; set; }
        public string jobTitle { get; set; }
        public int cityId { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public System.TimeSpan timeIn { get; set; }
        public System.TimeSpan timeOut { get; set; }
        public string picture { get; set; }
        public bool deleted { get; set; }
    }
}
