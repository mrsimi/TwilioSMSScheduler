namespace TwilioSmsScheduler;

public class UserCalendarConfiguration
{
    public static UserCalendarConfiguration Instance { get; } = new();

    public string OpeningTime { get; set; }
    public string ClosingTime { get; set; }
    public string CheckedDays { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime ExpiryDateTime { get; set; }
}