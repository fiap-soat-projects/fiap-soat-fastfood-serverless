using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace fiap.soat.fastfood.serverless.function;

public class CpfRequest
{
    public string? Cpf { get; set; }
}

public class ValidaCpf
{
    private readonly ILogger<ValidaCpf> _logger;

    public ValidaCpf(ILogger<ValidaCpf> logger)
    {
        _logger = logger;
    }

    //[Function("ValidaCpf")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processou um pedido.");

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        CpfRequest data = JsonConvert.DeserializeObject<CpfRequest>(requestBody);
        string cpf = data?.Cpf;

        if (string.IsNullOrEmpty(cpf))
        {
            return new BadRequestObjectResult("Por favor, passe um CPF no corpo da requisi��o.");
        }

        // --- L�gica de Valida��o/Consulta ---
        // **Aqui � onde voc� integraria com o AD, Cognito ou um banco de dados.**
        // Por exemplo, uma chamada para um sistema de identidade para verificar se o CPF existe e est� ativo.

        bool cpfValido = SimularValidacaoCPF(cpf);

        if (cpfValido)
        {
            _logger.LogInformation($"CPF {cpf} validado com sucesso.");
            // Retorna 200 OK com o CPF para o APIM us�-lo na gera��o do token.
            // O APIM precisa desta informa��o para inclu�-la no token.
            return new OkObjectResult(new
            {
                Status = "Validado",
                Cpf = cpf,
                UserId = $"user_{cpf}" // Um ID de usu�rio �nico
            });
        }
        else
        {
            _logger.LogWarning($"Tentativa de login falhou para o CPF: {cpf}");
            // Retorna 401 ou 403 se a valida��o falhar.
            return new UnauthorizedObjectResult(new { Status = "Falha na Autentica��o", Message = "CPF n�o encontrado ou inv�lido." });
        }

    }

    private static bool SimularValidacaoCPF(string cpf)
    {
        // Implemente a l�gica real (consulta a AD/DB).
        // Aqui, apenas um exemplo simples: CPFs que come�am com '123' s�o v�lidos.
        return cpf.Trim().StartsWith("123");
    }
}