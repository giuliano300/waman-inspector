using System;

public class InspectionPlanViewModel
{
    public int inspectionId { get; set; }
    public DateTime date { get; set; }
    public TimeSpan time { get; set; }
    public bool closed { get; set; }
    public string locationName { get; set; }
    public string locationAddress { get; set; }
    public string cityName { get; set; }
    public string entityName { get; set; }
    public string navigationUrl { get; set; }
}
