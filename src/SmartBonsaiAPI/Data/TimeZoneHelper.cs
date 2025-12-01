using System;

public static class TimeZoneHelper
{
    private static readonly TimeZoneInfo BrazilTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    /// <summary>
    /// Obtém a data/hora atual no fuso horário de Brasília (UTC-3)
    /// </summary>
    public static DateTime GetBrazilTime()
    {
        try
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, BrazilTimeZone);
        }
        catch
        {
            // Fallback para Windows ou sistemas que não reconhecem "America/Sao_Paulo"
            try
            {
                var brazilTz = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, brazilTz);
            }
            catch
            {
                // Último fallback: UTC-3 manualmente
                return DateTime.UtcNow.AddHours(-3);
            }
        }
    }

    /// <summary>
    /// Converte uma data/hora UTC para o fuso horário de Brasília
    /// </summary>
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

    /// <summary>
    /// Formata a data/hora atual de Brasília no formato brasileiro
    /// </summary>
    public static string GetBrazilTimeFormatted(string format = "dd/MM/yyyy HH:mm:ss")
    {
        return GetBrazilTime().ToString(format);
    }
}

