using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;

namespace TwilioSMSScheduler.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IConfiguration _configuration;
    private string clientId, redirectUrl, scope, authBaseUrl, clientSecret, requestTokenBaseUrl = string.Empty;
    
    public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;

        authBaseUrl = _configuration["GoogleAPI:AuthBaseUrl"];
        clientId = _configuration["GoogleAPI:ClientID"];
        redirectUrl = _configuration["GoogleAPI:RedirectUrl"];
        scope = _configuration["GoogleAPI:Scope"];
        clientSecret = _configuration["GoogleAPI:Secrets"];
        requestTokenBaseUrl = _configuration["GoogleAPI:RequestTokenUrl"];
    }

    [BindProperty]
    public List<string> CheckedDays {get; set;}

    [BindProperty]
    public List<string> AllDays {get; set;} = new List<string>
    {
            "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"
    };

    public bool IsConnected {get; set;} =  !string.IsNullOrEmpty(AppConfig.GetUserConfig().RefreshToken);
    public bool IsWorkHourSet {get; set;} = !string.IsNullOrEmpty(AppConfig.GetUserConfig().OpeningTime); 

    public async Task OnGet(string code)
    {
        if (string.IsNullOrEmpty(code))
        {
            string redirectLink = $"{authBaseUrl}?client_id={clientId}&redirect_uri={redirectUrl}&response_type=code&scope={scope}&access_type=offline";
            redirectLink = $"{authBaseUrl}?client_id={clientId}&" +
           $"redirect_uri={redirectUrl}&response_type=code&scope={scope}&access_type=offline";

            ViewData["RedirectLink"] = redirectLink;
        }

        else
        {
            string requestTokenUrl = $"{requestTokenBaseUrl}?client_id={clientId}&client_secret={clientSecret}&code={code}&grant_type=authorization_code&redirect_uri={redirectUrl}";
           
            var httpClient = new HttpClient();
            
            var response = await httpClient.PostAsync(requestTokenUrl, null);
           

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(content);

                string expireSecondsContent = jsonObj["expires_in"];
                string accesTokenContent = jsonObj["access_token"];
                string refreshTokenContent = jsonObj["refresh_token"];

                int expirySeconds;
                int.TryParse(expireSecondsContent, out expirySeconds);

                
                Dictionary<string, string> configValues = new Dictionary<string, string>();
                configValues.Add("AccessToken", accesTokenContent);
                configValues.Add("RefreshToken", AesOperation.EncryptString(_configuration["ConfigEncryptKey"], refreshTokenContent));
                configValues.Add("ExpiryDateTime", DateTime.Now.AddSeconds(expirySeconds).ToString());
                
                AppConfig.ModifyUserConfig(configValues);

                IsConnected = true;
            }
            

            Redirect("/");
        }
    }

    public void OnPost()
    {
        Dictionary<string, string> formValues = new Dictionary<string, string>();

        var form = Request.Form;
        formValues.Add("OpeningTime", form["OpeningTime"]);
        formValues.Add("ClosingTime", form["ClosingTime"]);
        formValues.Add("CheckedDays", string.Join(",", form["CheckedDays"]));

        AppConfig.ModifyUserConfig(formValues);
        IsWorkHourSet = true;
    }

    
}
