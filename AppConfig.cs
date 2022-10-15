using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwilioSMSScheduler
{
    public static class AppConfig
    {   
        public static string GetUserConfig()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(),"config.json");
            string json = File.ReadAllText(filePath);
            return json;
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

        public static bool IsConnectedToGoogle()
        {
            var filePath = Directory.GetCurrentDirectory()+"/config.json";
            string json = File.ReadAllText(filePath);
            dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            
            var refreshToken = jsonObj["RefreshToken"];
            if(string.IsNullOrEmpty(refreshToken.ToString()))
            {
                return false;
            }
            else 
            {
                return true;
            }
        }

        public static bool IsWorkHoursSet()
        {
            var filePath = Directory.GetCurrentDirectory()+"/config.json";
            string json = File.ReadAllText(filePath);
            dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            
            var refreshToken = jsonObj["OpeningTime"];
            if(string.IsNullOrEmpty(refreshToken.ToString()))
            {
                return false;
            }
            else 
            {
                return true;
            }
        }
    }
}