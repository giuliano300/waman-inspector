using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Waman_app.Models
{
    public class WamEntities
    {
        public int id { get; set; }
        public int entityTypeId { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string mobile { get; set; }
        public string email { get; set; }
        public string headOfEntity { get; set; }
        public string headOfEntityMobile { get; set; }
        public bool deleted { get; set; }
    }
}