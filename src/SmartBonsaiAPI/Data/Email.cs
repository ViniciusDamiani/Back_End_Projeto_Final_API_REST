using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

public interface IEmailSender
{
    Task<bool> SendAsync(string subject, string body, bool isHtml = false);
    Task<bool> SendTestEmailAsync();
    bool IsConfigured();
}

public class SmtpEmailSender : IEmailSender
{
    private readonly string _host;
    private readonly int _port;
    private readonly string _username;
    private readonly string _password;
    private readonly bool _enableSsl;
    private readonly string _from;
    private readonly string _to;

    public SmtpEmailSender()
    {
        _host = Environment.GetEnvironmentVariable("SMTP_HOST") ?? string.Empty;
        _port = int.TryParse(Environment.GetEnvironmentVariable("SMTP_PORT"), out var p) ? p : 587;
        _username = Environment.GetEnvironmentVariable("SMTP_USER") ?? string.Empty;
        _password = Environment.GetEnvironmentVariable("SMTP_PASS") ?? string.Empty;
        _enableSsl = (Environment.GetEnvironmentVariable("SMTP_ENABLE_SSL") ?? "true").Equals("true", StringComparison.OrdinalIgnoreCase);
        _from = Environment.GetEnvironmentVariable("SMTP_FROM") ?? _username;
        _to = Environment.GetEnvironmentVariable("SMTP_TO") ?? string.Empty;
    }

    public bool IsConfigured()
    {
        return !string.IsNullOrWhiteSpace(_host) 
            && !string.IsNullOrWhiteSpace(_from) 
            && !string.IsNullOrWhiteSpace(_to);
    }

    public async Task<bool> SendAsync(string subject, string body, bool isHtml = false)
    {
        if (!IsConfigured())
        {
            throw new InvalidOperationException(
                "Configuração de email incompleta. Verifique as variáveis de ambiente: SMTP_HOST, SMTP_FROM, SMTP_TO");
        }

        try
        {
            using var client = new SmtpClient(_host, _port)
            {
                EnableSsl = _enableSsl,
                Credentials = string.IsNullOrEmpty(_username)
                    ? CredentialCache.DefaultNetworkCredentials
                    : new NetworkCredential(_username, _password),
                Timeout = 30000 // 30 segundos
            };

            using var message = new MailMessage(_from, _to)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            await client.SendMailAsync(message);
            return true;
        }
        catch (SmtpException ex)
        {
            throw new InvalidOperationException($"Erro ao enviar email via SMTP: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro inesperado ao enviar email: {ex.Message}", ex);
        }
    }

    public async Task<bool> SendTestEmailAsync()
    {
        var subject = "Teste de Email - SmartBonsai API";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f9f9f9; padding: 20px; border-radius: 0 0 5px 5px; }}
        .success {{ color: #4CAF50; font-weight: bold; }}
        .info {{ background-color: #e7f3ff; padding: 15px; border-left: 4px solid #2196F3; margin: 15px 0; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🌿 SmartBonsai API</h1>
        </div>
        <div class='content'>
            <p class='success'>✓ Email de teste enviado com sucesso!</p>
            <div class='info'>
                <p><strong>Data/Hora:</strong> {TimeZoneHelper.GetBrazilTimeFormatted()}</p>
                <p><strong>Servidor SMTP:</strong> {_host}:{_port}</p>
                <p><strong>Remetente:</strong> {_from}</p>
                <p><strong>Destinatário:</strong> {_to}</p>
            </div>
            <p>Se você recebeu este email, significa que a configuração de email está funcionando corretamente.</p>
        </div>
        <div class='footer'>
            <p>SmartBonsai API - Sistema de Monitoramento de Bonsai</p>
        </div>
    </div>
</body>
</html>";

        return await SendAsync(subject, htmlBody, isHtml: true);
    }
}


