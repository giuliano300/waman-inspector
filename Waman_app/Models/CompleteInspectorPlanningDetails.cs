using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Waman_app.Models
{
    public class CompleteInspectorPlanningDetails
    {
        public Workers worker { get; set; }
        public InspectorPlanningDetails det { get; set; }
        public string jobTypeName { get; set; }

        public bool? wpFind {  get; set; }
    }
}