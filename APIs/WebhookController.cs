using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Twilio.AspNet.Core;
using Twilio.TwiML;

namespace TwilioSMSScheduler.APIs
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        ILogger<WebhookController> _logger;
        private string clientId, clientSecret, requestTokenBaseUrl, calendarUrl = string.Empty;
        public WebhookController(IConfiguration configuration, ILogger<WebhookController> logger)
        {
            _logger = logger;
            _configuration = configuration;

            clientId = _configuration["GoogleAPI:ClientID"];
            clientSecret = _configuration["GoogleAPI:Secrets"];
            requestTokenBaseUrl = _configuration["GoogleAPI:RequestTokenUrl"];
            calendarUrl = _configuration["GoogleAPI:CalendarUrl"];
        }

       
        [HttpPost]
        public async Task<IActionResult> IncomingMsgTrigger()
        {
            string smsBody = Request.Form["Body"];
            string smsFrom = Request.Form["From"];

            string accessToken = "";
            var appConfigDetails = AppConfig.GetUserConfig();

            //EXTRACT APPOINTMENT
            AppointmentDetails appointmentDetails = new AppointmentDetails(smsBody,"UTC+1", smsFrom);

            //CHECK DAYS AND TIME. 
            var DateTimeOpen = DateTime.ParseExact(appConfigDetails.OpeningTime, "HH:mm", CultureInfo.InvariantCulture);
            var DateTimeClose = DateTime.ParseExact(appConfigDetails.ClosingTime, "HH:mm", CultureInfo.InvariantCulture);

            var dayOfWeek = appointmentDetails.start.dateTime.DayOfWeek.ToString();

            if(!appConfigDetails.CheckedDays.Split(",").Contains(dayOfWeek))
            {
                var response = new MessagingResponse();
                response.Message($"Business does not take appointment on {dayOfWeek}");
                return new TwiMLResult(response);
            }

            if(!(appointmentDetails.start.dateTime.TimeOfDay >= DateTimeOpen.TimeOfDay && appointmentDetails.end.dateTime.TimeOfDay <= DateTimeClose.TimeOfDay))
            {
                var response = new MessagingResponse();
                response.Message($"Business only takes appointment between {DateTimeOpen.TimeOfDay} and {DateTimeClose.TimeOfDay}");
                return new TwiMLResult(response);
            } 


            var httpClient = new HttpClient();

            //GET NEW TOKEN IF TOKEN EXPIRED
            DateTime expiryDate =DateTime.Parse(appConfigDetails.ExpiryDateTime);
            if(DateTime.Now > expiryDate)
            {
                string decryptedRefreshToken = AesOperation.DecryptString(_configuration["ConfigEncryptKey"], appConfigDetails.RefreshToken);
                string refreshTokenUrl = $"{requestTokenBaseUrl}?client_id={clientId}&client_secret={clientSecret}&grant_type=refresh_token&refresh_token={decryptedRefreshToken}";
                var response = await httpClient.PostAsync(refreshTokenUrl, null);

                if(response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(content);

                    string expireSecondsContent = jsonObj["expires_in"];
                    string accesTokenContent = jsonObj["access_token"];

                    int expirySeconds;
                    int.TryParse(expireSecondsContent, out expirySeconds);


                    Dictionary<string, string> configValues = new Dictionary<string, string>();
                    configValues.Add("AccessToken", accesTokenContent);
                    configValues.Add("ExpiryDateTime", DateTime.Now.AddSeconds(expirySeconds).ToString());

                    AppConfig.ModifyUserConfig(configValues);

                    accessToken = accesTokenContent;
                }
                else 
                {
                    _logger.LogError(await response.Content.ReadAsStringAsync());   
                    var result = new MessagingResponse();
                    result.Message($"An Error occured while try to create an appointment for you. Kindly try agian later");
                    return new TwiMLResult(result);
                }
            }
            else
            {
                accessToken = appConfigDetails.AccessToken;
            }
            

           
            //INSERT EVENT INTO THE GOOGLE CALENDAR
            var requestBody = new StringContent(JsonConvert.SerializeObject(appointmentDetails));
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

             //CHECK IF NO EVENT EXIST IN THAT DATE AND TIME
            var allEvents = await httpClient.GetAsync(calendarUrl);
            if(allEvents.IsSuccessStatusCode)
            {
                var allEventsResponse = await allEvents.Content.ReadAsStringAsync();
                var deserializedEvents = JsonConvert.DeserializeObject<CalendarResponse>(allEventsResponse);

                if(deserializedEvents.items.Any(m => m.start.dateTime == appointmentDetails.start.dateTime 
                    && m.end.dateTime == appointmentDetails.end.dateTime))
                {
                    var result = new MessagingResponse();
                    result.Message("There is already an appointment for this time slot. Kindly select another time.");
                    return new TwiMLResult(result);
                }
            }
            else 
            {
                _logger.LogError(await allEvents.Content.ReadAsStringAsync());
                var result = new MessagingResponse();
                result.Message($"An Error occured while try to create an appointment for you. Kindly try agian later");
                return new TwiMLResult(result);
            }

            var createCalendarResponse = await httpClient.PostAsync(calendarUrl, requestBody);

            if (createCalendarResponse.IsSuccessStatusCode)
            {
                var result = new MessagingResponse();
                result.Message($"An appointment has been created for you. We expect to see you soon on {appointmentDetails.start.dateTime}");
                return new TwiMLResult(result);
            }
            else
            {
                _logger.LogError(await createCalendarResponse.Content.ReadAsStringAsync());   
                var result = new MessagingResponse();
                result.Message($"An Error occured while try to create an appointment for you. Kindly try agian later");
                return new TwiMLResult(result);
            }
        }
    }
}