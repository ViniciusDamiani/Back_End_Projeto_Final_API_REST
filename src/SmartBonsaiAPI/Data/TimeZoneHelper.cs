using System;

public static class TimeZoneHelper
{
    private static readonly TimeZoneInfo BrazilTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    public static DateTime GetBrazilTime()
    {
        try
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, BrazilTimeZone);
        }
        catch
        {
            try
            {
                var brazilTz = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, brazilTz);
            }
            catch
            {
                return DateTime.UtcNow.AddHours(-3);
            }
        }
    }

    public static DateTime ToBrazilTime(DateTime utcDateTime)
    {
        try
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, BrazilTimeZone);
        }
        catch
        {
            try
            {
                var brazilTz = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, brazilTz);
            }
            catch
            {
                return utcDateTime.AddHours(-3);
            }
        }
    }

    public static string GetBrazilTimeFormatted(string format = "dd/MM/yyyy HH:mm:ss")
    {
        return GetBrazilTime().ToString(format);
    }
}

