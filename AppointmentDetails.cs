using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TwilioSMSScheduler
{
    public class AppointmentDetails
    {
        public string summary;
        public string description;
        public int colorId;
        public CalendarDateTime start;
        public CalendarDateTime end;
        public AppointmentDetails(string smsBody, string timeZone, string senderPhoneNo)
        {
            string appointmentTemplate = "make an appointment for";
            string timeTemplate = "on the";
            string [] postionQualifier = new string[] {"rd", "st", "nd", "th"};
            string [] monthNames = DateTimeFormatInfo.CurrentInfo.MonthNames;

            string appointmentSummary = Regex.Match(smsBody, @$"{appointmentTemplate}(.+?){timeTemplate}").Groups[1].Value;
            string apointmentDate = Regex.Match(smsBody, @"on the (.+?) at").Groups[1].Value;
            string appointmentTime = smsBody.Substring(smsBody.LastIndexOf("at")).Replace("at", "").ToUpper();

            smsBody = smsBody.Replace("I would", senderPhoneNo);
            var appointmentDateArray = apointmentDate.Split(" ");

            string appointmentDay = appointmentDateArray[0];
            for (int i = 0; i < postionQualifier.Length; i++)
            {
                appointmentDay = appointmentDay.Replace(postionQualifier[i], "");
            }


            int monthNumber=0;
            for (int i = 0; i < monthNames.Length; i++)
            {
                if(monthNames[i].ToUpper().Contains(appointmentDateArray[2].ToUpper()))
                {
                    monthNumber = i+1;
                }
            }

            appointmentTime = appointmentTime.Replace("AM", "-AM").Replace(" ", "");
            appointmentTime = appointmentTime.Replace("PM","-PM").Replace(" ", "");;
            appointmentTime = appointmentTime.Replace("NOON", "-PM");
            
            string resultAppointmentTime = $"{DateTime.Now.Year}-{monthNumber.ToString().PadLeft(2, '0')}-{appointmentDay.PadLeft(2, '0')} {appointmentTime.PadLeft(5, '0')}";

            var date = DateTime.ParseExact(resultAppointmentTime, "yyyy-MM-dd hh-tt", 
                                  CultureInfo.InvariantCulture);
            
            this.summary = appointmentSummary; 
            this.description = smsBody;
            this.colorId = 3;
            this.start = new CalendarDateTime 
            {
                dateTime = date, 
                timeZone = timeZone
            };
            this.end = new CalendarDateTime
            {
                dateTime = date.AddHours(1), 
                timeZone = timeZone
            };

        }
    }

    public class CalendarDateTime
    {
        public DateTime dateTime { get; set; } 
        public string timeZone {get; set;}
    }
}