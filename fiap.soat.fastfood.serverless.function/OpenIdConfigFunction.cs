using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace fiap.soat.fastfood.serverless.function;

public class OpenIdConfigFunction
{
    private readonly JwtService _jwtService;
    private readonly string _issuer;
    public OpenIdConfigFunction(JwtService jwtService, IConfiguration settings)
    {
        _jwtService = jwtService;
        _issuer = settings["ISSUER"] ?? throw new
            ArgumentNullException("ISSUER");
    }

    [Function("OpenIdConfig")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = ".well-known/openid-configuration")] HttpRequestData req, FunctionContext ctx)
    {
        //string baseUrl = req.Url.GetLeftPart(System.UriPartial.Authority);
        string baseUrl = _issuer;
        var config = _jwtService.GetOpenIdConfig(baseUrl);
        var res = req.CreateResponse(System.Net.HttpStatusCode.OK);
        res.Headers.Add("Content-Type", "application/json");

        await res.WriteStringAsync(JsonConvert.SerializeObject(config));
        return res;
    }
}