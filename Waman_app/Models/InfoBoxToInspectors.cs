using System;

namespace Waman_app.Models
{
    public class InfoBoxToInspectors
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public DateTime date { get; set; }
        public bool deleted { get; set; }
    }
}
