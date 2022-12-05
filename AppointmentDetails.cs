using System.Globalization;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace TwilioSmsScheduler;

public class AppointmentDetails
{
    [JsonProperty(PropertyName = "summary")]
    public string Summary { get; set; }
    
    [JsonProperty(PropertyName = "description")]
    public string Description { get; set; }
    
    [JsonProperty(PropertyName = "colorId")]
    public int ColorId { get; set; }
    
    [JsonProperty(PropertyName = "start")]
    public CalendarDateTime Start { get; set; }
    
    [JsonProperty(PropertyName = "end")]
    public CalendarDateTime End { get; set; }
}

public class CalendarDateTime
{
    [JsonProperty(PropertyName = "dateTime")]
    public DateTime DateTime { get; set; }
    
    [JsonProperty(PropertyName = "timeZone")]
    public string TimeZone { get; set; }
}