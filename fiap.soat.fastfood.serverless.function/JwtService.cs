using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;

namespace fiap.soat.fastfood.serverless.function
{


    public class JwtService
    {
        private readonly string _keyVaultUri;
        private readonly string _keyName;
        private readonly string _issuer;
        private readonly string _audience;
        public JwtService(IConfiguration config)
        {
            _keyVaultUri = config["KEYVAULT_URI"] ?? throw new
            ArgumentNullException("KEYVAULT_URI");
            _keyName = config["JWT_KEY_NAME"] ?? throw new
            ArgumentNullException("JWT_KEY_NAME");

            _issuer = config["ISSUER"] ?? throw new
            ArgumentNullException("ISSUER");
            _audience = config["AUDIENCE"] ?? throw new
            ArgumentNullException("AUDIENCE");
        }
        private static string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
        }
        public async Task<string> GenerateTokenAsync(string cpf, int minutesValid = 15)
        {
            var credential = new DefaultAzureCredential();
            var keyClient = new KeyClient(new Uri(_keyVaultUri), credential);
            var key = await keyClient.GetKeyAsync(_keyName);
            var crypto = new CryptographyClient(key.Value.Id, credential);
            var now = DateTimeOffset.UtcNow;

            var header = new
            {
                alg = "RS256",
                typ = "JWT",
                kid = key.Value.Id.ToString()
            };
            var payload = new
            {
                sub = cpf,
                iss = _issuer,
                aud = _audience,
                iat = now.ToUnixTimeSeconds(),
                exp = now.AddMinutes(minutesValid).ToUnixTimeSeconds()
            };
            string headerEncoded = Base64UrlEncode(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(header)));
            string payloadEncoded = Base64UrlEncode(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload)));
            string dataToSign = $"{headerEncoded}.{payloadEncoded}";
            // O SignDataAsync com RS256 calcula internamente o hash SHA256 e     assina
            var signResult = await crypto.SignDataAsync(SignatureAlgorithm.RS256, Encoding.UTF8.GetBytes(dataToSign));
            string signatureEncoded = Base64UrlEncode(signResult.Signature);
            string jwt = $"{dataToSign}.{signatureEncoded}";
            return jwt;
        }
        public async Task<object> GetJwksAsync()
        {
            var credential = new DefaultAzureCredential();
            var keyClient = new KeyClient(new Uri(_keyVaultUri), credential);
            var key = await keyClient.GetKeyAsync(_keyName);
            var jwk = key.Value.Key;
            string n = Base64UrlEncode(jwk.N);
            string e = Base64UrlEncode(jwk.E);
            string kid = key.Value.Id.ToString();
            var jwkObj = new
            {
                keys = new[]
            {
                new
                {
                    kty = "RSA",
                    use = "sig",
                    kid = kid,
                    n = n,
                    e = e,
                    alg = "RS256"
                }
}
            };
            return jwkObj;
        }
        public object GetOpenIdConfig(string baseUrl)
        {
            return new
            {
                issuer = _issuer,
                jwks_uri = $"{baseUrl}/.well-known/jwks.json"
            };
        }
    }
}
