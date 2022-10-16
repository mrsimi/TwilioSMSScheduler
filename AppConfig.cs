using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace TwilioSMSScheduler
{
    public static class AppConfig
    {   
        public static AppConfigFields GetUserConfig()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(),"config.json");
            string json = File.ReadAllText(filePath);

            return Newtonsoft.Json.JsonConvert.DeserializeObject<AppConfigFields>(json);
        }

        public static void ModifyUserConfig(Dictionary<string, string> configValues)
        {
            var filePath = Directory.GetCurrentDirectory()+"/config.json";
            string json = File.ReadAllText(filePath);
            dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            
            foreach (var item in configValues)
            {
                jsonObj[item.Key] = item.Value;
            }

            string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText("config.json", output);
        }
    }

    public class AppConfigFields
    {
        public string OpeningTime {get; set;} 
        public string ClosingTime {get; set;}
        public string CheckedDays {get; set;} 
        public string AccessToken {get; set;} 
        public string RefreshToken {get; set;}
        public string ExpiryDateTime {get; set;}
    }
}