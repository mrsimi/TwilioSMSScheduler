using System.Globalization;
using System.Text.RegularExpressions;

namespace TwilioSmsScheduler;

public static class AppointmentDetailsExtractor
{
    public static AppointmentDetails Extract(string smsBody, string timeZone, string senderPhoneNo)
    {
        const string appointmentTemplate = "make an appointment for";
        const string timeTemplate = "on the";
        string[] positionQualifier = {"rd", "st", "nd", "th"};
        var monthNames = DateTimeFormatInfo.CurrentInfo.MonthNames;

        var appointmentSummary = Regex.Match(smsBody, @$"{appointmentTemplate}(.+?){timeTemplate}").Groups[1].Value;
        var appointmentDate = Regex.Match(smsBody, @"on the (.+?) at").Groups[1].Value;
        var appointmentTime = smsBody.Substring(smsBody.LastIndexOf("at")).Replace("at", "").ToUpper();

        smsBody = smsBody.Replace("I would", senderPhoneNo);
        var appointmentDateArray = appointmentDate.Split(" ");

        var appointmentDay = appointmentDateArray[0];
        for (var i = 0; i < positionQualifier.Length; i++)
        {
            appointmentDay = appointmentDay.Replace(positionQualifier[i], "");
        }

        var monthNumber = 0;
        for (var i = 0; i < monthNames.Length; i++)
        {
            if (monthNames[i].ToUpper().Contains(appointmentDateArray[2].ToUpper()))
            {
                monthNumber = i + 1;
            }
        }

        appointmentTime = appointmentTime.Replace("AM", "-AM").Replace(" ", "");
        appointmentTime = appointmentTime.Replace("PM", "-PM").Replace(" ", "");
        appointmentTime = appointmentTime.Replace("NOON", "-PM");

        var resultAppointmentTime = $"{DateTime.Now.Year}" +
                                    $"-{monthNumber.ToString().PadLeft(2, '0')}" +
                                    $"-{appointmentDay.PadLeft(2, '0')}" +
                                    $" {appointmentTime.PadLeft(5, '0')}";

        var date = DateTime.ParseExact(
            resultAppointmentTime, 
            "yyyy-MM-dd hh-tt",
            CultureInfo.InvariantCulture
        );

        return new AppointmentDetails
        {
            Summary = appointmentSummary,
            Description = smsBody,
            ColorId = 3,
            Start = new CalendarDateTime
            {
                DateTime = date,
                TimeZone = timeZone
            },
            End = new CalendarDateTime
            {
                DateTime = date.AddHours(1),
                TimeZone = timeZone
            }
        };
    }
}