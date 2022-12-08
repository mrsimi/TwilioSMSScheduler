namespace TwilioSmsScheduler;

public class UserConfiguration
{
    public static UserConfiguration Instance { get; } = new();

    public string OpeningTime { get; set; }
    public string ClosingTime { get; set; }
    public string CheckedDays { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime ExpiryDateTime { get; set; }
}