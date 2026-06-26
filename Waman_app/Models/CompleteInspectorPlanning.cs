using System.Collections.Generic;

namespace Waman_app.Models
{
    public class CompleteInspectorPlanning
    {
        public Inspectors inspector { get; set; }
        public InspectorCodes inspectorCode { get; set; }

        public InspectorPlannings inspectorPlanning { get; set; }
        public InspectionActivities inspectionActivity { get; set; }
        public Locations location { get; set; }
        public WamEntities entity { get; set; }
        public Cities city { get; set; }

        public List<WorkersAndWorkerPlanningsDto> workers { get; set; }
        public List<CompleteInspectorPlanningDetails> details { get; set; }
    }
}