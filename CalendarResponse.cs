using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwilioSMSScheduler
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Creator
    {
        public string email { get; set; }
        public bool self { get; set; }
    }

    public class DefaultReminder
    {
        public string method { get; set; }
        public int minutes { get; set; }
    }

    public class End
    {
        public DateTime dateTime { get; set; }
        public string timeZone { get; set; }
    }

    public class ExtendedProperties
    {
        public Private @private { get; set; }
        public Shared shared { get; set; }
    }

    public class Item
    {
        public string kind { get; set; }
        public string etag { get; set; }
        public string id { get; set; }
        public string status { get; set; }
        public string htmlLink { get; set; }
        public DateTime created { get; set; }
        public DateTime updated { get; set; }
        public string summary { get; set; }
        public string description { get; set; }
        public string location { get; set; }
        public Creator creator { get; set; }
        public Organizer organizer { get; set; }
        public Start start { get; set; }
        public End end { get; set; }
        public string iCalUID { get; set; }
        public int sequence { get; set; }
        public Reminders reminders { get; set; }
        public string eventType { get; set; }
        public ExtendedProperties extendedProperties { get; set; }
        public string colorId { get; set; }
    }

    public class Organizer
    {
        public string email { get; set; }
        public bool self { get; set; }
    }

    public class Override
    {
        public string method { get; set; }
        public int minutes { get; set; }
    }

    public class Private
    {
        public string activityInsights { get; set; }
    }

    public class Reminders
    {
        public bool useDefault { get; set; }
        public List<Override> overrides { get; set; }
    }

    public class CalendarResponse
    {
        public string kind { get; set; }
        public string etag { get; set; }
        public string summary { get; set; }
        public DateTime updated { get; set; }
        public string timeZone { get; set; }
        public string accessRole { get; set; }
        public List<DefaultReminder> defaultReminders { get; set; }
        public string nextSyncToken { get; set; }
        public List<Item> items { get; set; }
    }

    public class Shared
    {
        public string activityInsights { get; set; }
    }

    public class Start
    {
        public DateTime dateTime { get; set; }
        public string timeZone { get; set; }
    }


}