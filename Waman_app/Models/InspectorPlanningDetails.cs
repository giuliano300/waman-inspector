using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Waman_app.Models
{
    public class InspectorPlanningDetails
    {
        public int id { get; set; }
        public int inspectorPlanningId { get; set; }
        public int workerId { get; set; }
        public bool? timeSheet { get; set; }
        public bool? foundAtWork { get; set; }
		public string otherIssues { get; set; }
		public string signature { get; set; }
		public bool deleted { get; set; }
    }
}