using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

public interface IEmailSender
{
    Task SendAsync(string subject, string body);
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

        // Logar configuração básica (mas não imprime senha)
        Console.WriteLine($"[Email] Config SMTP -> Host={_host}, Port={_port}, From={_from}, To={_to}, SSL={_enableSsl}, UserSet={(string.IsNullOrEmpty(_username) ? "no" : "yes")}");
    }

    public async Task SendAsync(string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(_host) || string.IsNullOrWhiteSpace(_from) || string.IsNullOrWhiteSpace(_to))
        {
            // Configuração insuficiente, não enviar, apenas logar e sair silenciosamente
            Console.WriteLine("[Email] Configuração SMTP incompleta. Verifique as variáveis de ambiente: SMTP_HOST, SMTP_FROM, SMTP_TO");
            return;
        }

        try
        {
            Console.WriteLine($"[Email] Tentativa de envio: '{subject}' -> {_to} via {_host}:{_port} SSL={_enableSsl}");
            using var client = new SmtpClient(_host, _port)
            {
                EnableSsl = _enableSsl,
                Credentials = string.IsNullOrEmpty(_username)
                    ? CredentialCache.DefaultNetworkCredentials
                    : new NetworkCredential(_username, _password),
                Timeout = 30000 // 30 segundos de timeout
            };

            using var message = new MailMessage(_from, _to)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            await client.SendMailAsync(message);
            Console.WriteLine($"[Email] Email enviado com sucesso: {subject}");
        }
        catch (Exception ex)
        {
            // Log do erro mas não interrompe a execução
            Console.WriteLine($"[Email] Erro ao enviar email: {ex.Message}");
            Console.WriteLine($"[Email] Detalhes: {ex}");
        }
    }
}


