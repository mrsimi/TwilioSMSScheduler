using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace TwilioSmsScheduler.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> logger;
    private readonly IConfiguration configuration;
    private readonly HttpClient httpClient;
    private readonly string clientId, redirectUrl, scope, authBaseUrl, clientSecret, requestTokenBaseUrl;
    private readonly UserCalendarConfiguration userCalendarConfig;

    public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration, HttpClient httpClient)
    {
        this.logger = logger;
        this.configuration = configuration;
        this.httpClient = httpClient;

        authBaseUrl = this.configuration["GoogleApi:AuthBaseUrl"];
        clientId = this.configuration["GoogleApi:ClientID"];
        redirectUrl = this.configuration["GoogleApi:RedirectUrl"];
        scope = this.configuration["GoogleApi:Scope"];
        clientSecret = this.configuration["GoogleApi:Secret"];
        requestTokenBaseUrl = this.configuration["GoogleApi:RequestTokenUrl"];
        userCalendarConfig = UserCalendarConfiguration.Instance;
        IsConnected = !string.IsNullOrEmpty(userCalendarConfig.RefreshToken);
        IsWorkHourSet = !string.IsNullOrEmpty(userCalendarConfig.OpeningTime);
    }

    [BindProperty] public List<string> CheckedDays { get; set; }

    public string[] AllDays { get; set; } =
    {
        "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"
    };

    public bool IsConnected { get; set; }
    public bool IsWorkHourSet { get; set; }
    public string RedirectLink { get; set; }

    public async Task OnGet(string code)
    {
        RedirectLink = authBaseUrl +
                       $"?client_id={clientId}&" +
                       $"redirect_uri={redirectUrl}" +
                       "&response_type=code" +
                       $"&scope={scope}" +
                       "&access_type=offline";

        if (code == null) return;
        
        var requestTokenUrl = requestTokenBaseUrl +
                              $"?client_id={clientId}" +
                              $"&client_secret={clientSecret}" +
                              $"&code={code}" +
                              "&grant_type=authorization_code" +
                              $"&redirect_uri={redirectUrl}";

        var response = await httpClient.PostAsync(requestTokenUrl, null);

        if (!response.IsSuccessStatusCode)
        {
            return;
        }

        var content = await response.Content.ReadAsStringAsync();
        dynamic jsonObj = JsonConvert.DeserializeObject(content);

        userCalendarConfig.AccessToken = jsonObj["access_token"];
        userCalendarConfig.RefreshToken = jsonObj["refresh_token"];
        userCalendarConfig.ExpiryDateTime = DateTime.Now.AddSeconds(int.Parse((string) jsonObj["expires_in"]));

        IsConnected = true;
    }

    public async Task OnPost()
    {
        var form = await Request.ReadFormAsync();
        userCalendarConfig.OpeningTime = form["OpeningTime"];
        userCalendarConfig.ClosingTime = form["ClosingTime"];
        userCalendarConfig.CheckedDays = string.Join(",", form["CheckedDays"]);

        IsWorkHourSet = true;
    }
}