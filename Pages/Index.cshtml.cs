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
    private readonly UserConfiguration userConfiguration;

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
        userConfiguration = UserConfiguration.Instance;
        IsConnected = !string.IsNullOrEmpty(userConfiguration.RefreshToken);
        IsWorkHourSet = !string.IsNullOrEmpty(userConfiguration.OpeningTime);
    }

    [BindProperty] public List<string> CheckedDays { get; set; }

    public string[] AllDays { get; } =
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

        userConfiguration.AccessToken = jsonObj["access_token"];
        userConfiguration.RefreshToken = jsonObj["refresh_token"];
        userConfiguration.ExpiryDateTime = DateTime.Now.AddSeconds(int.Parse((string) jsonObj["expires_in"]));

        IsConnected = true;
    }

    public async Task OnPost()
    {
        var form = await Request.ReadFormAsync();
        userConfiguration.OpeningTime = form["OpeningTime"];
        userConfiguration.ClosingTime = form["ClosingTime"];
        userConfiguration.CheckedDays = string.Join(",", form["CheckedDays"]);

        IsWorkHourSet = true;
    }
}