using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;

namespace fiap.soat.fastfood.serverless.function;

public class LoginFunction
{
    private readonly JwtService _jwtService;
    public LoginFunction(JwtService jwtService)
    {
        _jwtService = jwtService;
    }

    [Function("Login")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "login")] HttpRequestData req, FunctionContext ctx)
    {
        var body = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(body);
        string cpf = data?.cpf;
        var response = req.CreateResponse();

        if (string.IsNullOrWhiteSpace(cpf) || !
        CpfUtils.IsValidCpf((string)cpf))
        {
            response.StatusCode = System.Net.HttpStatusCode.BadRequest;
            await response.WriteStringAsync(JsonConvert.SerializeObject(new
            { error = "cpf-invalid" }));
            return response;
        }
        // TODO: substituir pela chamada ao sistema legado que confirma se o  CPF existe / está ativo
        bool exists = true; // simulando
        if (!exists)
        {
            response.StatusCode = System.Net.HttpStatusCode.Unauthorized;
            await response.WriteStringAsync(JsonConvert.SerializeObject(new
            { error = "cpf-not-found" }));
            return response;
        }
        int minutes = int.Parse(System.Environment.GetEnvironmentVariable("TOKEN_EXP_MINUTES") ?? "15");
        string token = await _jwtService.GenerateTokenAsync(cpf, minutes);
        response.StatusCode = System.Net.HttpStatusCode.OK;
        response.Headers.Add("Content-Type", "application/json");

        await response.WriteStringAsync(JsonConvert.SerializeObject(new
        {
            access_token = token,
            token_type = "Bearer",
            expires_in = minutes * 60
        }));
        return response;
    }
}