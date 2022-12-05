using Newtonsoft.Json;

namespace TwilioSmsScheduler;

public class Start
{
    [JsonProperty(PropertyName = "dateTime")]
    public DateTime DateTime { get; set; }
    
    [JsonProperty(PropertyName = "timeZone")]
    public string TimeZone { get; set; }
}

public class End
{
    [JsonProperty(PropertyName = "dateTime")]
    public DateTime DateTime { get; set; }
    
    [JsonProperty(PropertyName = "timeZone")]
    public string TimeZone { get; set; }
}

public class Item
{
    [JsonProperty(PropertyName = "start")]
    public Start Start { get; set; }
    
    [JsonProperty(PropertyName = "end")]
    public End End { get; set; }
}

public class CalendarResponse
{
    [JsonProperty(PropertyName = "items")]
    public List<Item> Items { get; set; }
}