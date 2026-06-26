using System;

namespace Waman_app.Models
{
    public class TrackInspectorDemo
    {
        public int id { get; set; }
        public int inspectorId { get; set; }
        public DateTime date { get; set; }
        public TimeSpan? start { get; set; }
        public TimeSpan? end { get; set; }
        public string gpsUrl { get; set; }
        public bool deleted { get; set; }
        public int trackStatusId { get; set; }
        public bool? blocked { get; set; }
        public DateTime lastUpdate { get; set; }
    }
}