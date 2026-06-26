using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Waman_app.Models
{
    public class Workers
    {
        public int id { get; set; }
        public string idCardNumber { get; set; }
        public string name { get; set; }
        public string lastName { get; set; }
        public string mobile { get; set; }
        public string email { get; set; }
        public int jobTypeId { get; set; }
        public string picture { get; set; }
        public bool deleted { get; set; }
    }
}