namespace Waman_app.Models
{
    public class Inspectors
    {
        public int id { get; set; }
        public string name { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string pwd { get; set; }
        public string mobile { get; set; }
        public int inspectorCodeId { get; set; }
        public bool deleted { get; set; }
        public string authToken { get; set; }

    }
}
