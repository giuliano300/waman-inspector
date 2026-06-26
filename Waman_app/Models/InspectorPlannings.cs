using System;

namespace Waman_app.Models
{
    public class InspectorPlannings
    {
        public int id { get; set; }
        public int inspectorId { get; set; }
        public int cityId { get; set; }
        public DateTime date { get; set; }
        public TimeSpan time { get; set; }
        public int locationId { get; set; }
        public int entityId { get; set; }
        public string gpsTrackFile { get; set; }
        public int inspectionActivityId { get; set; }
        public bool closed { get; set; }
        public bool confirm { get; set; }
        public string comments { get; set; }
        public string reportFilePath { get; set; }
        public string entityComment { get; set; }
        public string signature { get; set; }
        public string inspectorAbsenceComment { get; set; }
        public bool deleted { get; set; }
        public bool sendReportToAdministration { get; set; }
    }
}