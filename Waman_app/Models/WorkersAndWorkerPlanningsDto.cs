using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Waman_app.Models
{
    public class WorkersAndWorkerPlanningsDto
    {
        public int workerId { get; set; }
        public string jobTypeName { get; set; }
        public string name { get; set; }
        public string lastName { get; set; }
        public string idCardNumber { get; set; }
        public DateTime startWork { get; set; }
        public DateTime endWork { get; set; }
    }
}