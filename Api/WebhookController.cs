using System.Globalization;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Twilio.AspNet.Core;
using Twilio.TwiML;

namespace TwilioSmsScheduler.Api;

[ApiController]
[Route("api/[controller]")]
public class WebhookController : ControllerBase
{
    private readonly ILogger<WebhookController> logger;
    private readonly HttpClient httpClient;
    private readonly string clientId, clientSecret, requestTokenBaseUrl, calendarUrl;

    public WebhookController(IConfiguration configuration, ILogger<WebhookController> logger, HttpClient httpClient)
    {
        this.logger = logger;
        this.httpClient = httpClient;
        clientId = configuration["GoogleApi:ClientID"];
        clientSecret = configuration["GoogleApi:Secret"];
        requestTokenBaseUrl = configuration["GoogleApi:RequestTokenUrl"];
        calendarUrl = configuration["GoogleApi:CalendarUrl"];
    }

    [HttpPost]
    public async Task<IActionResult> IncomingMessage()
    {
        string messageBody = Request.Form["Body"];
        string messageFrom = Request.Form["From"];

        var userCalendarConfig = UserCalendarConfiguration.Instance;

        // Extract appointment
        var appointmentDetails = AppointmentDetails.Extract(messageBody, "UTC+1", messageFrom);

        // Check days and time 
        var dateTimeOpen = DateTime.ParseExact(userCalendarConfig.OpeningTime, "HH:mm", CultureInfo.InvariantCulture);
        var dateTimeClose = DateTime.ParseExact(userCalendarConfig.ClosingTime, "HH:mm", CultureInfo.InvariantCulture);

        var dayOfWeek = appointmentDetails.Start.DateTime.DayOfWeek.ToString();

        if (!userCalendarConfig.CheckedDays.Split(",").Contains(dayOfWeek))
        {
            var response = new MessagingResponse();
            response.Message($"Business does not take appointment on {dayOfWeek}");
            return new TwiMLResult(response);
        }

        if (!(appointmentDetails.Start.DateTime.TimeOfDay >= dateTimeOpen.TimeOfDay &&
              appointmentDetails.End.DateTime.TimeOfDay <= dateTimeClose.TimeOfDay))
        {
            return new MessagingResponse()
                .Message(
                    $"Business only takes appointment between {dateTimeOpen.TimeOfDay} and {dateTimeClose.TimeOfDay}")
                .ToTwiMLResult();
        }

        var accessToken = await GetAccessToken(userCalendarConfig);

        // Insert event into the Google Calendar
        var requestBody = new StringContent(JsonConvert.SerializeObject(appointmentDetails));
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        // Check if no event exist in that date and time
        var allEventsResponse = await httpClient.GetAsync(calendarUrl);
        var allEventsBody = await allEventsResponse.Content.ReadAsStringAsync();
        if (!allEventsResponse.IsSuccessStatusCode)
        {
            logger.LogError("Failed to get all events: {AllEventsResponseBody}", allEventsBody);
            return new MessagingResponse()
                .Message("An Error occured while try to create an appointment for you. Kindly try again later.")
                .ToTwiMLResult();
        }

        var deserializedEvents = JsonConvert.DeserializeObject<CalendarResponse>(allEventsBody);
        if (deserializedEvents.Items
            .Where(m => m.Start != null && m.End != null)
            .Any(m => m.Start.DateTime == appointmentDetails.Start.DateTime
                      && m.End.DateTime == appointmentDetails.End.DateTime))
        {
            return new MessagingResponse()
                .Message("There is already an appointment for this time slot. Kindly select another time.")
                .ToTwiMLResult();
        }

        var createCalendarResponse = await httpClient.PostAsync(calendarUrl, requestBody);
        var createCalendarBody = await createCalendarResponse.Content.ReadAsStringAsync();

        if (!createCalendarResponse.IsSuccessStatusCode)
        {
            logger.LogError("Failed to create calendar event: {CreateCalendarBody}", createCalendarBody);
            return new MessagingResponse()
                .Message("An Error occured while try to create an appointment for you. Kindly try again later.")
                .ToTwiMLResult();
        }

        return new MessagingResponse()
            .Message(
                $"An appointment has been created for you. We expect to see you soon on {appointmentDetails.Start.DateTime}")
            .ToTwiMLResult();
    }

    private async Task<string> GetAccessToken(UserCalendarConfiguration userCalendarConfig)
    {
        // Use existing token if not expired
        if (DateTime.Now <= userCalendarConfig.ExpiryDateTime)
        {
            return userCalendarConfig.AccessToken;
        }

        // Get new token if expired
        var refreshTokenUrl = $"{requestTokenBaseUrl}" +
                              $"?client_id={clientId}" +
                              $"&client_secret={clientSecret}" +
                              "&grant_type=refresh_token" +
                              $"&refresh_token={userCalendarConfig.RefreshToken}";

        var response = await httpClient.PostAsync(refreshTokenUrl, null);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Refreshing token failed: {RefreshTokenResponseBody}",
                await response.Content.ReadAsStringAsync());
            throw new Exception("Refreshing token failed");
        }

        var content = await response.Content.ReadAsStringAsync();
        dynamic jsonObj = JsonConvert.DeserializeObject(content);

        var expirySeconds = int.Parse(jsonObj["expires_in"]);
        userCalendarConfig.ExpiryDateTime = DateTime.Now.AddSeconds(expirySeconds);

        userCalendarConfig.AccessToken = jsonObj["access_token"];
        return userCalendarConfig.AccessToken;
    }
}