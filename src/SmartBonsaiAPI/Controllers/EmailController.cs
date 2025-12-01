using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

[ApiController]
[Route("api/email")]
public class EmailController : ControllerBase
{
    private readonly IEmailSender _emailSender;

    public EmailController(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    /// <summary>
    /// Verifica se o serviço de email está configurado
    /// </summary>
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        var isConfigured = _emailSender.IsConfigured();
        
        // Diagnóstico das variáveis (sem mostrar valores sensíveis)
        var envVars = new
        {
            SMTP_HOST = GetEnvStatus("SMTP_HOST"),
            SMTP_PORT = GetEnvStatus("SMTP_PORT"),
            SMTP_USER = GetEnvStatus("SMTP_USER"),
            SMTP_PASS = GetEnvStatus("SMTP_PASS", hideValue: true),
            SMTP_FROM = GetEnvStatus("SMTP_FROM"),
            SMTP_TO = GetEnvStatus("SMTP_TO"),
            SMTP_ENABLE_SSL = GetEnvStatus("SMTP_ENABLE_SSL")
        };

        var loadedFrom = EnvLoader.GetLoadedFrom();
        
        return Ok(new
        {
            isConfigured = isConfigured,
            message = isConfigured 
                ? "Serviço de email configurado corretamente" 
                : "Serviço de email não configurado. Verifique as variáveis de ambiente.",
            envFile = loadedFrom ?? "Nenhum arquivo .env encontrado",
            variables = envVars,
            instructions = !isConfigured ? new
            {
                message = "Crie um arquivo .env na raiz do projeto com as seguintes variáveis:",
                required = new[] { "SMTP_HOST", "SMTP_FROM", "SMTP_TO" },
                optional = new[] { "SMTP_PORT (padrão: 587)", "SMTP_USER", "SMTP_PASS", "SMTP_ENABLE_SSL (padrão: true)" },
                example = @"
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USER=seu-email@gmail.com
SMTP_PASS=sua-senha
SMTP_FROM=seu-email@gmail.com
SMTP_TO=destinatario@example.com
SMTP_ENABLE_SSL=true"
            } : null
        });
    }

    private object GetEnvStatus(string key, bool hideValue = false)
    {
        var value = Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrEmpty(value))
        {
            return new { exists = false, value = (string?)null };
        }
        
        return new 
        { 
            exists = true, 
            value = hideValue ? "***" : value,
            isEmpty = string.IsNullOrWhiteSpace(value)
        };
    }

    /// <summary>
    /// Envia um email de teste
    /// </summary>
    [HttpPost("test")]
    public async Task<IActionResult> SendTestEmail()
    {
        try
        {
            if (!_emailSender.IsConfigured())
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Serviço de email não configurado. Configure as variáveis de ambiente: SMTP_HOST, SMTP_FROM, SMTP_TO"
                });
            }

            var result = await _emailSender.SendTestEmailAsync();
            
            if (result)
            {
                return Ok(new
                {
                    success = true,
                    message = "Email de teste enviado com sucesso!"
                });
            }
            else
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Falha ao enviar email de teste"
                });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "Erro ao enviar email de teste",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Envia um email customizado (para testes)
    /// </summary>
    [HttpPost("send")]
    public async Task<IActionResult> SendEmail([FromBody] EmailRequestDto request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Subject) || string.IsNullOrWhiteSpace(request.Body))
        {
            return BadRequest(new
            {
                success = false,
                message = "Subject e Body são obrigatórios"
            });
        }

        try
        {
            if (!_emailSender.IsConfigured())
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Serviço de email não configurado"
                });
            }

            var result = await _emailSender.SendAsync(request.Subject, request.Body, request.IsHtml ?? false);
            
            if (result)
            {
                return Ok(new
                {
                    success = true,
                    message = "Email enviado com sucesso!"
                });
            }
            else
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Falha ao enviar email"
                });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "Erro ao enviar email",
                error = ex.Message
            });
        }
    }
}

public class EmailRequestDto
{
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool? IsHtml { get; set; }
}

