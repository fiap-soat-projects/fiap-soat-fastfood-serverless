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
            return new BadRequestObjectResult("Por favor, passe um CPF no corpo da requisição.");
        }

        // --- Lógica de Validação/Consulta ---
        // **Aqui é onde você integraria com o AD, Cognito ou um banco de dados.**
        // Por exemplo, uma chamada para um sistema de identidade para verificar se o CPF existe e está ativo.

        bool cpfValido = SimularValidacaoCPF(cpf);

        if (cpfValido)
        {
            _logger.LogInformation($"CPF {cpf} validado com sucesso.");
            // Retorna 200 OK com o CPF para o APIM usá-lo na geração do token.
            // O APIM precisa desta informação para incluí-la no token.
            return new OkObjectResult(new
            {
                Status = "Validado",
                Cpf = cpf,
                UserId = $"user_{cpf}" // Um ID de usuário único
            });
        }
        else
        {
            _logger.LogWarning($"Tentativa de login falhou para o CPF: {cpf}");
            // Retorna 401 ou 403 se a validação falhar.
            return new UnauthorizedObjectResult(new { Status = "Falha na Autenticação", Message = "CPF não encontrado ou inválido." });
        }

    }

    private static bool SimularValidacaoCPF(string cpf)
    {
        // Implemente a lógica real (consulta a AD/DB).
        // Aqui, apenas um exemplo simples: CPFs que começam com '123' são válidos.
        return cpf.Trim().StartsWith("123");
    }
}