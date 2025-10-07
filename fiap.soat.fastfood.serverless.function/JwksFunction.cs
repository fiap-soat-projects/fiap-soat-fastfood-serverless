using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;

namespace fiap.soat.fastfood.serverless.function;

public class JwksFunction
{
    private readonly JwtService _jwtService;
    public JwksFunction(JwtService jwtService)
    {
        _jwtService = jwtService;
    }

    [Function("Jwks")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = ".well-known/jwks.json")] HttpRequestData req, FunctionContext ctx)
    {
        var jwks = await _jwtService.GetJwksAsync();
        var res = req.CreateResponse(System.Net.HttpStatusCode.OK);
        res.Headers.Add("Content-Type", "application/json");
        await res.WriteStringAsync(JsonConvert.SerializeObject(jwks));
        return res;
    }
}