namespace Waman_app.Models
{
    public class InspectionEndDto
    {
        public string signature { get; set; }
        public bool confirm { get; set; }
        public bool sendReportToAdministration { get; set; }
        public string entityComment { get; set; }
		public string inspectorAbsenceComment { get; set; }
	}
}